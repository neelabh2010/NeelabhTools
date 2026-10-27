using System.Data;
using Microsoft.Data.SqlClient;
using NeelabhCoreTools.SqlClient;

namespace NeelabhCoreTools.Tests;

// Integration tests use only literals, table variables, and connection-local temporary objects.
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DBTOOLS_TEST_CONNECTION_STRING")))
            Skip = "Set DBTOOLS_TEST_CONNECTION_STRING to run SQL Server integration tests.";
    }
}

public class DbToolsSqlServerTests
{
    private static DbTools Create(bool safe = true) => new(
        Environment.GetEnvironmentVariable("DBTOOLS_TEST_CONNECTION_STRING")!, CommandType.Text)
    { UseSafeBehavior = safe };

    private static void Success(ResultInfo result) => Assert.False(result.HasError, result.SystemMessage);

    [SqlServerFact]
    public void LegacyExecuteDefaultsAndExplicitConnectionFlagsArePreserved()
    {
        using var db = Create(false);
        db.Command = "SELECT 42";
        Assert.Equal("42", db.ExecuteScalar().Result);
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
        db.Open();
        Assert.Equal("42", db.ExecuteScaler(false, false).Result);
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        Assert.Equal("42", db.ExecuteScalar(false, true).Result);
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
        Success(db.Execute(true, false));
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        var result = db.Execute();
        Success(result);
        Assert.Equal("", result.Result);
        Assert.Null(result.Object);
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
    }

    [SqlServerFact]
    public void ExistingExplicitTransactionPatternStillCommits()
    {
        using var db = Create(false);
        db.Open(true);
        db.Command = "SELECT 1";
        Success(db.Execute(false, false));
        var transaction = db.Transaction;
        db.Commit();
        Assert.Same(transaction, db.Transaction); // Legacy callers can inspect/dispose their handle.
        db.Close();
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
    }

    [SqlServerFact]
    public void LegacyDefaultExecutionStillClosesAnActiveTransaction()
    {
        using var db = Create(false);
        db.Open(true);
        var transaction = db.Transaction;
        db.Command = "SELECT 1";
        Success(db.Execute());
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
        Assert.Null(transaction.Connection);
    }

    [SqlServerFact]
    public void SafeModeRejectsMismatchedPublicStateWithoutClosingCallerResources()
    {
        using var db = Create();
        db.Open(true);
        var original = db.DbConnection;
        using var replacement = new SqlConnection(original.ConnectionString);
        replacement.Open();
        db.DbConnection = replacement;
        try
        {
            db.Command = "SELECT 1";
            var result = db.Execute();
            Assert.True(result.HasError);
            Assert.IsType<InvalidOperationException>(db.LastException);
            Assert.Equal(ConnectionState.Open, original.State);
            Assert.Equal(ConnectionState.Open, replacement.State);
            Assert.Same(original, db.Transaction.Connection);
        }
        finally { db.DbConnection = original; }
        db.Rollback();
    }

    [SqlServerFact]
    public void SafeExecutionPreservesCallerConnectionAndCommitsOrRollsBackChanges()
    {
        using var db = Create();
        db.Open();
        db.Command = "CREATE TABLE #DbToolsReview (Id int NOT NULL)";
        Success(db.Execute());
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        db.BeginTransaction();
        var transaction = db.Transaction;
        db.Command = "INSERT INTO #DbToolsReview VALUES (1)";
        var insert = db.ExecuteNonQuery();
        Success(insert);
        Assert.Equal(1, insert.Value);
        Assert.Same(transaction, db.Transaction);
        Assert.Throws<InvalidOperationException>(() => db.BeginTransaction());
        db.Commit();
        Assert.Null(db.Transaction);
        Assert.Null(db.DbCommand.Transaction);
        db.BeginTransaction();
        db.Command = "INSERT INTO #DbToolsReview VALUES (2)";
        Success(db.Execute());
        db.Rollback();
        db.Command = "SELECT COUNT(*) FROM #DbToolsReview";
        Assert.Equal(1, db.ExecuteScalar<int>().Value);
        Assert.Null(db.Transaction);
    }

    [SqlServerFact]
    public void OpenTrueAndDefaultExecutionKeepSafeTransactionAlive()
    {
        using var db = Create();
        db.Open(true);
        db.Command = "SELECT @@TRANCOUNT";
        Assert.Equal(1, db.ExecuteScalar<int>().Value);
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        db.Close(true);
        Assert.Null(db.Transaction);
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
    }

    [SqlServerFact]
    public void SafeStandaloneExecutionClosesOnlyItsOwnConnection()
    {
        using var db = Create();
        db.Command = "SELECT 1";
        Success(db.Execute());
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
        db.Open();
        Success(db.Execute());
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        db.Close();
    }

    [SqlServerFact]
    public void SafeCloseClosesConnectionEvenWhenCommitFails()
    {
        using var db = Create();
        db.Open(true);
        db.Transaction.Rollback(); // Simulate transaction completion outside the helper.
        Assert.Throws<InvalidOperationException>(() => db.Close(true));
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
        Assert.Null(db.Transaction);
    }

    [SqlServerFact]
    public void SafeParameterValuesDistinguishEmptyAndSqlNull()
    {
        using var db = Create();
        db.AddParameter(new SqlParameter("@text", SqlDbType.NVarChar, 50) { Value = "" });
        db.Command = "SELECT CASE WHEN @text IS NULL THEN -1 ELSE DATALENGTH(@text) END";
        Assert.Equal(0, db.ExecuteScalar<int>().Value);
        db.AssignParam("@text", null);
        Assert.Equal(-1, db.ExecuteScalar<int>().Value);
        db.AssignParam("@text", DBNull.Value);
        Assert.Equal(-1, db.ExecuteScalar<int>().Value);
    }

    [SqlServerFact]
    public void TypedScalarsDistinguishNoRowSqlNullEmptyAndNativeTypes()
    {
        using var db = Create();
        db.Command = "SELECT CAST(NULL AS int)";
        var sqlNull = db.ExecuteScalar<int?>();
        Success(sqlNull);
        Assert.False(sqlNull.HasValue);
        Assert.True(sqlNull.IsDbNull);
        Assert.Null(sqlNull.Value);
        db.Command = "SELECT 1 WHERE 1 = 0";
        var noRow = db.ExecuteScalar<int>();
        Assert.False(noRow.HasValue);
        Assert.False(noRow.IsDbNull);
        db.Command = "SELECT ''";
        var empty = db.ExecuteScalar<string>();
        Assert.True(empty.HasValue);
        Assert.Equal("", empty.Value);
        db.Command = "SELECT CAST(12.34 AS decimal(10,2))";
        Assert.Equal(12.34m, db.ExecuteScalar<decimal>().Value);
        db.Command = "SELECT 42";
        Assert.Equal(42L, db.ExecuteScalar<long?>().Value);
        db.Command = "SELECT CAST(0x010203 AS varbinary(3))";
        Assert.Equal(new byte[] { 1, 2, 3 }, db.ExecuteScalar<byte[]>().Value);
        db.Command = "SELECT 'not-an-integer'";
        var invalid = db.ExecuteScalar<int>();
        Assert.True(invalid.HasError);
        Assert.IsType<FormatException>(invalid.Exception);
    }

    [SqlServerFact]
    public void TableDataSetAndReaderKeepTheirLegacyContracts()
    {
        using var db = Create(false);
        db.Command = "SELECT 1 AS Id; SELECT 2 AS Id";
        var data = db.ExecuteDataSet();
        Success(data);
        Assert.Equal(2, Assert.IsType<DataSet>(data.Object).Tables.Count);
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
        db.Open();
        var table = db.ExecuteTable();
        Success(table);
        Assert.Equal(1, table.Table.Rows[0]["Id"]);
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        using (var reader = db.ExecuteReader()) Assert.True(reader.Read());
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
    }

    [SqlServerFact]
    public async Task ConnectionProbesDoNotDisturbAnActiveTransaction()
    {
        using var db = Create();
        db.Open(true);
        var transaction = db.Transaction;
        Assert.True(db.TestConnection(), db.LastException?.Message);
        Assert.True(await db.TestConnectionAsync(), db.LastException?.Message);
        Assert.Same(transaction, db.Transaction);
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        db.Command = "SELECT @@TRANCOUNT";
        Assert.Equal(1, db.ExecuteScalar<int>().Value);
        db.Rollback();
    }

    [SqlServerFact]
    public async Task AsyncOperationsSupportTransactionsResultsAndReaders()
    {
        using var db = Create();
        await db.OpenAsync(true);
        db.Command = "DECLARE @items TABLE (Id int); INSERT INTO @items VALUES (1), (2)";
        var inserted = await db.ExecuteNonQueryAsync();
        Success(inserted);
        Assert.Equal(2, inserted.Value);
        db.Command = "SELECT 42";
        Assert.Equal("42", (await db.ExecuteScalarAsync()).Result);
        Assert.Equal(42, (await db.ExecuteScalarAsync<int>()).Value);
        await using (var reader = await db.ExecuteReaderAsync())
        {
            Assert.True(await reader.ReadAsync());
            Assert.Equal(42, reader.GetInt32(0));
        }
        await db.CommitAsync();
        Assert.Null(db.Transaction);
        await db.BeginTransactionAsync();
        await db.RollbackAsync();
        Assert.Null(db.Transaction);
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
    }

    [SqlServerFact]
    public async Task CancellationClosesOwnedConnectionButPreservesCallerTransaction()
    {
        using var db = Create();
        db.Command = "WAITFOR DELAY '00:00:10'; SELECT 1";
        using (var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300)))
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                db.ExecuteScalarAsync<int>(cancellationToken: cancellation.Token));
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
        await db.OpenAsync(true);
        using (var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300)))
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                db.ExecuteAsync(cancellationToken: cancellation.Token));
        Assert.Equal(ConnectionState.Open, db.DbConnection.State);
        Assert.NotNull(db.Transaction);
        await db.RollbackAsync();
    }

    [SqlServerFact]
    public void SqlErrorsRetainDiagnosticsAndClearOnNextSuccess()
    {
        using var db = Create();
        db.Command = "THROW 50001, 'DbTools regression test', 1";
        var failed = db.ExecuteNonQuery();
        Assert.True(failed.HasError);
        Assert.Equal(50001, failed.ErrorNo);
        Assert.IsType<SqlException>(failed.Exception);
        Assert.Same(failed.Exception, db.LastException);
        db.Command = "SELECT 1";
        Success(db.Execute());
        Assert.Null(db.LastException);
    }

    [SqlServerFact]
    public void ExplicitParameterMetadataSupportsOutputValues()
    {
        using var db = Create();
        var output = db.AddParameter(new SqlParameter("@value", SqlDbType.Decimal)
        { Precision = 10, Scale = 2, Direction = ParameterDirection.Output });
        db.Command = "SET @value = 12.34";
        Success(db.Execute());
        Assert.Equal(12.34m, output.Value);
    }

    [SqlServerFact]
    public void DisposalClosesConnectionWithAnActiveTransaction()
    {
        var db = Create();
        db.Open(true);
        var connection = db.DbConnection;
        db.Dispose();
        Assert.Equal(ConnectionState.Closed, connection.State);
        Assert.Null(db.Transaction);
        db.Dispose();
    }
}
