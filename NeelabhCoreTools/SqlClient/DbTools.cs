using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace NeelabhCoreTools.SqlClient
{
    // Keep the published fields and layout unchanged.
    public struct DbParam
    {
        public string Param;
        public object Value;
        public SqlDbType? DbType;
    }

    /// <summary>
    /// SQL Server helper. Instances, commands, and readers must not be used concurrently.
    /// Existing semantics are retained unless UseSafeBehavior is enabled.
    /// Dispose releases the currently assigned connection, command, and transaction.
    /// Public setters remain available for compatibility; replacing a resource does not dispose it.
    /// </summary>
    public class DbTools : IDisposable
    {
        private bool disposed;

        public SqlConnection DbConnection { get; set; } = new SqlConnection();
        public SqlCommand DbCommand { get; set; } = new SqlCommand();
        public SqlTransaction Transaction { get; set; } = null;

        /// <summary>
        /// Opts into SQL NULL normalization, parameter upserts, state validation, and preserving
        /// connections opened by callers or used by active transactions. Defaults to false for
        /// compatibility. Close() always explicitly closes the connection in either mode.
        /// </summary>
        public bool UseSafeBehavior { get; set; }

        /// <summary>The original exception from the latest result-returning operation or connection test.</summary>
        public Exception LastException { get; private set; }

        public int CommandTimeout
        {
            get => DbCommand.CommandTimeout;
            set => DbCommand.CommandTimeout = value;
        }

        public DbTools(string connectionString = "", CommandType commandType = CommandType.StoredProcedure)
        {
            DbConnection.ConnectionString = connectionString;
            DbCommand.Connection = DbConnection;
            DbCommand.CommandType = commandType;
        }

        public bool TestConnection()
        {
            LastException = null;
            try
            {
                ThrowIfDisposed();
                using var probe = (SqlConnection)((ICloneable)DbConnection).Clone();
                probe.Open();
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            LastException = null;
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                ThrowIfDisposed();
                using var probe = (SqlConnection)((ICloneable)DbConnection).Clone();
                await probe.OpenAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                LastException = ex;
                cancellationToken.ThrowIfCancellationRequested();
                return false;
            }
        }

        public void Open(bool transactionHandling = false)
        {
            ThrowIfDisposed();
            if (UseSafeBehavior) ValidateState();
            if (DbConnection.State != ConnectionState.Open) DbConnection.Open();
            DbCommand.Connection = DbConnection;
            if (transactionHandling) BeginTransaction();
        }

        public async Task OpenAsync(bool transactionHandling = false, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (UseSafeBehavior) ValidateState();
            if (DbConnection.State != ConnectionState.Open)
                await DbConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            DbCommand.Connection = DbConnection;
            if (transactionHandling)
                await BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        }

        public void BeginTransaction()
        {
            ThrowIfDisposed();
            if (UseSafeBehavior) ValidateTransactionStart();
            Transaction = DbConnection.BeginTransaction();
            DbCommand.Transaction = Transaction;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (UseSafeBehavior) ValidateTransactionStart();
            Transaction = (SqlTransaction)await DbConnection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            DbCommand.Transaction = Transaction;
        }

        public void Commit()
        {
            ThrowIfDisposed();
            if (UseSafeBehavior) RequireTransaction();
            // On failure retain the transaction so the caller can still attempt rollback.
            Transaction.Commit();
            if (UseSafeBehavior) ReleaseTransaction();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            RequireTransaction();
            await Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            ReleaseTransaction();
        }

        public void Rollback()
        {
            ThrowIfDisposed();
            if (Transaction == null) return;
            Transaction.Rollback();
            if (UseSafeBehavior) ReleaseTransaction();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (Transaction == null) return;
            await Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            ReleaseTransaction();
        }

        public void Close(bool commitTransaction = false)
        {
            ThrowIfDisposed();
            if (!UseSafeBehavior)
            {
                if (commitTransaction) Commit();
                if (DbConnection.State == ConnectionState.Open) DbConnection.Close();
                return;
            }
            try
            {
                if (commitTransaction) Commit();
            }
            finally
            {
                try { DbConnection.Close(); }
                finally { ReleaseTransaction(); }
            }
        }

        public string Command { set => DbCommand.CommandText = value; }

        public string SQLAction
        {
            set
            {
                ThrowIfDisposed();
                if (UseSafeBehavior)
                    UpsertParameter(new DbParam { Param = "sql_action", Value = value });
                else
                    DbCommand.Parameters.AddWithValue("sql_action", value);
            }
        }

        /// <summary>
        /// Legacy mode appends parameters. Safe mode updates matching names and preserves unrelated
        /// parameters (including SQLAction). Use SetParameters to replace the entire set.
        /// </summary>
        public List<DbParam> Parameters
        {
            set
            {
                ThrowIfDisposed();
                foreach (var item in value)
                {
                    if (UseSafeBehavior) UpsertParameter(item);
                    else
                    {
                        DbCommand.Parameters.AddWithValue(item.Param, item.Value);
                        if (item.DbType != null)
                            DbCommand.Parameters[item.Param].SqlDbType = (SqlDbType)item.DbType;
                    }
                }
            }
        }

        public void AssignParam(string parameterName, object value)
        {
            ThrowIfDisposed();
            if (UseSafeBehavior) SetParameterValue(parameterName, value);
            else DbCommand.Parameters[parameterName].Value = value == null || value.ToString() == "" ? null : value;
        }

        /// <summary>Assigns SQL NULL for null and preserves empty strings. The parameter must exist.</summary>
        public void SetParameterValue(string parameterName, object value)
        {
            ThrowIfDisposed();
            DbCommand.Parameters[parameterName].Value = value ?? DBNull.Value;
        }

        /// <summary>
        /// Adds a parameter with explicit SqlDbType, Size, Precision, Scale, Direction or TypeName.
        /// Duplicate names are rejected. Null input values become SQL NULL in either mode.
        /// Omit optional stored-procedure parameters when server defaults are required.
        /// </summary>
        public SqlParameter AddParameter(SqlParameter parameter)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(parameter);
            ArgumentException.ThrowIfNullOrWhiteSpace(parameter.ParameterName);
            if (FindParameter(parameter.ParameterName) != null)
                throw new ArgumentException("A parameter with this name already exists.", nameof(parameter));
            if (parameter.Value == null && parameter.Direction != ParameterDirection.Output &&
                parameter.Direction != ParameterDirection.ReturnValue)
                parameter.Value = DBNull.Value;
            return DbCommand.Parameters.Add(parameter);
        }

        public SqlParameter AddParameter(DbParam parameter) => AddParameter(CreateParameter(parameter));

        /// <summary>Replaces all parameters, normalizes nulls and rejects duplicate names.</summary>
        public void SetParameters(IEnumerable<DbParam> parameters)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(parameters);
            var replacement = new List<SqlParameter>();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in parameters)
            {
                var parameter = CreateParameter(item);
                if (!names.Add(parameter.ParameterName.TrimStart('@')))
                    throw new ArgumentException("Duplicate parameter names are not allowed.", nameof(parameters));
                replacement.Add(parameter);
            }
            DbCommand.Parameters.Clear();
            DbCommand.Parameters.AddRange(replacement.ToArray());
        }

        public void ClearParameters()
        {
            ThrowIfDisposed();
            DbCommand.Parameters.Clear();
        }

        public ResultInfo Execute(bool openConnection = true, bool closeConnection = true) =>
            Run(new ResultInfo(), _ => DbCommand.ExecuteNonQuery(), openConnection, closeConnection);

        /// <summary>Compatibility alias, retained without an obsolete warning.</summary>
        [Obsolete]
        public ResultInfo ExecuteScaler(bool openConnection = true, bool closeConnection = true)
        {
            return ExecuteScalar(openConnection, closeConnection);
        }

        public ResultInfo ExecuteScalar(bool openConnection = true, bool closeConnection = true) =>
            Run(new ResultInfo(), result => result.Result = DbCommand.ExecuteScalar()?.ToString() ?? "",
                openConnection, closeConnection);

        public DbResult<int> ExecuteNonQuery(bool openConnection = true, bool closeConnection = true) =>
            Run(new DbResult<int>(), result =>
            {
                result.Value = DbCommand.ExecuteNonQuery();
                result.HasValue = true;
            }, openConnection, closeConnection);

        /// <summary>
        /// No row: HasValue=false, IsDbNull=false. SQL NULL: HasValue=false, IsDbNull=true.
        /// Exact types are preserved; convertible types use invariant culture. Failed conversion is an error.
        /// </summary>
        public DbResult<T> ExecuteScalar<T>(bool openConnection = true, bool closeConnection = true) =>
            Run(new DbResult<T>(), result => SetScalarValue(result, DbCommand.ExecuteScalar()),
                openConnection, closeConnection);

        public Task<ResultInfo> ExecuteAsync(bool openConnection = true, bool closeConnection = true,
            CancellationToken cancellationToken = default) =>
            RunAsync(new ResultInfo(), async (_, token) =>
            {
                await DbCommand.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }, openConnection, closeConnection, cancellationToken);

        public Task<ResultInfo> ExecuteScalarAsync(bool openConnection = true, bool closeConnection = true,
            CancellationToken cancellationToken = default) =>
            RunAsync(new ResultInfo(), async (result, token) =>
            {
                var value = await DbCommand.ExecuteScalarAsync(token).ConfigureAwait(false);
                result.Result = value?.ToString() ?? "";
            }, openConnection, closeConnection, cancellationToken);

        public Task<DbResult<int>> ExecuteNonQueryAsync(bool openConnection = true, bool closeConnection = true,
            CancellationToken cancellationToken = default) =>
            RunAsync(new DbResult<int>(), async (result, token) =>
            {
                result.Value = await DbCommand.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                result.HasValue = true;
            }, openConnection, closeConnection, cancellationToken);

        public Task<DbResult<T>> ExecuteScalarAsync<T>(bool openConnection = true, bool closeConnection = true,
            CancellationToken cancellationToken = default) =>
            RunAsync(new DbResult<T>(), async (result, token) =>
            {
                SetScalarValue(result, await DbCommand.ExecuteScalarAsync(token).ConfigureAwait(false));
            }, openConnection, closeConnection, cancellationToken);

        /// <summary>The caller opens the connection and disposes the returned reader.</summary>
        public SqlDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);

        public SqlDataReader ExecuteReader(CommandBehavior behavior)
        {
            ThrowIfDisposed();
            if (UseSafeBehavior) ValidateState();
            return DbCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// The caller opens the connection and disposes the reader. CloseConnection behavior explicitly
        /// transfers connection closure to the reader, even in safe mode.
        /// </summary>
        public Task<SqlDataReader> ExecuteReaderAsync(CommandBehavior behavior = CommandBehavior.Default,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            if (UseSafeBehavior) ValidateState();
            return DbCommand.ExecuteReaderAsync(behavior, cancellationToken);
        }

        public ResultInfo ExecuteTable() => Run(new ResultInfo(), result =>
        {
            result.Table = new DataTable();
            using var adapter = new SqlDataAdapter(DbCommand);
            adapter.Fill(result.Table);
        }, false, false);

        public ResultInfo ExecuteDataSet() => Run(new ResultInfo(), result =>
        {
            var dataSet = new DataSet();
            using var adapter = new SqlDataAdapter(DbCommand);
            adapter.Fill(dataSet);
            result.Object = dataSet;
        }, false, false);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || disposed) return;
            try { ReleaseTransaction(); }
            finally
            {
                try { DbCommand?.Dispose(); }
                finally
                {
                    try { DbConnection?.Dispose(); }
                    finally { disposed = true; }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private TResult Run<TResult>(TResult result, Action<TResult> operation,
            bool openConnection, bool closeConnection) where TResult : ResultInfo
        {
            LastException = null;
            bool openedHere = false;
            try
            {
                ThrowIfDisposed();
                if (UseSafeBehavior) ValidateState();
                if (openConnection)
                {
                    openedHere = DbConnection.State != ConnectionState.Open;
                    Open();
                }
                operation(result);
            }
            catch (Exception ex) { RecordError(result, ex); }
            finally { FinishOperation(result, closeConnection, openedHere); }
            return result;
        }

        private async Task<TResult> RunAsync<TResult>(TResult result,
            Func<TResult, CancellationToken, Task> operation, bool openConnection, bool closeConnection,
            CancellationToken cancellationToken) where TResult : ResultInfo
        {
            LastException = null;
            // Pre-cancelled calls must not touch caller resources, including in legacy mode.
            cancellationToken.ThrowIfCancellationRequested();
            bool openedHere = false;
            bool cancelled = false;
            try
            {
                ThrowIfDisposed();
                if (UseSafeBehavior) ValidateState();
                if (openConnection)
                {
                    openedHere = DbConnection.State != ConnectionState.Open;
                    await OpenAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                await operation(result, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { cancelled = true; throw; }
            catch (Exception ex)
            {
                RecordError(result, ex);
                cancelled = cancellationToken.IsCancellationRequested;
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally { FinishOperation(result, closeConnection, openedHere, cancelled); }
            return result;
        }

        private void FinishOperation(ResultInfo result, bool closeConnection, bool openedHere, bool cancelled = false)
        {
            if (!closeConnection || disposed) return;
            if (UseSafeBehavior && (!openedHere || HasActiveTransaction())) return;
            try { Close(); }
            catch (Exception ex)
            {
                if (!UseSafeBehavior && !cancelled) throw;
                // Preserve the original error or cancellation if cleanup also fails.
                if (!result.HasError && !cancelled) RecordError(result, ex);
            }
        }

        private void RecordError(ResultInfo result, Exception exception)
        {
            LastException = exception;
            result.SetError(exception.Message, exception is SqlException sql ? sql.Number : 0, true);
            if (result is IDbResult dbResult) dbResult.SetException(exception);
        }

        private static void SetScalarValue<T>(DbResult<T> result, object value)
        {
            if (value == null) return;
            if (value == DBNull.Value) { result.IsDbNull = true; return; }
            if (value is T typed) result.Value = typed;
            else
            {
                var target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                result.Value = (T)Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
            }
            result.HasValue = true;
        }

        private static SqlParameter CreateParameter(DbParam item)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Param);
            var parameter = new SqlParameter { ParameterName = item.Param };
            if (item.DbType.HasValue) parameter.SqlDbType = item.DbType.Value;
            parameter.Value = item.Value ?? DBNull.Value;
            return parameter;
        }

        private SqlParameter FindParameter(string name)
        {
            string normalized = name.TrimStart('@');
            foreach (SqlParameter parameter in DbCommand.Parameters)
                if (string.Equals(parameter.ParameterName.TrimStart('@'), normalized, StringComparison.OrdinalIgnoreCase))
                    return parameter;
            return null;
        }

        private void UpsertParameter(DbParam item)
        {
            var parameter = CreateParameter(item);
            var existing = FindParameter(item.Param);
            if (existing == null) DbCommand.Parameters.Add(parameter);
            else
            {
                if (item.DbType.HasValue) existing.SqlDbType = item.DbType.Value;
                existing.Value = parameter.Value;
            }
        }

        private bool HasActiveTransaction() =>
            Transaction?.Connection != null || DbCommand?.Transaction?.Connection != null;

        private void ValidateState()
        {
            if (DbConnection == null || DbCommand == null)
                throw new InvalidOperationException("DbConnection and DbCommand must be assigned.");
            var transaction = Transaction ?? DbCommand.Transaction;
            if (transaction != null && transaction.Connection != DbConnection)
                throw new InvalidOperationException("The transaction is completed or belongs to another connection.");
            if (Transaction != null && DbCommand.Transaction != null && Transaction != DbCommand.Transaction)
                throw new InvalidOperationException("The command and helper reference different transactions.");
            DbCommand.Connection = DbConnection;
            DbCommand.Transaction = transaction;
            Transaction = transaction;
        }

        private void ValidateTransactionStart()
        {
            ValidateState();
            if (HasActiveTransaction()) throw new InvalidOperationException("A transaction is already active.");
            if (DbConnection.State != ConnectionState.Open)
                throw new InvalidOperationException("Open the connection before starting a transaction.");
        }

        private void RequireTransaction()
        {
            if (UseSafeBehavior) ValidateState();
            if (Transaction?.Connection == null)
                throw new InvalidOperationException("There is no active transaction to commit.");
        }

        private void ReleaseTransaction()
        {
            var transaction = Transaction;
            Transaction = null;
            if (DbCommand != null && DbCommand.Transaction == transaction) DbCommand.Transaction = null;
            transaction?.Dispose();
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
    }
}
