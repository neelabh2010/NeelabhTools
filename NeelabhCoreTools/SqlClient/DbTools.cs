using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace NeelabhCoreTools.SqlClient
{
    public struct DbParam
    {
        public string Param;
        public object Value;
        public SqlDbType? DbType;
    }

    public class DbTools
    {
        public SqlConnection DbConnection { get; set; } = new SqlConnection();

        public SqlCommand DbCommand { get; set; } = new SqlCommand();

        public SqlTransaction Transaction { get; set; } = null;

        public DbTools(string connectionString = "", CommandType commandType = CommandType.StoredProcedure)
        {
            DbConnection.ConnectionString = connectionString;
            DbCommand.Connection = DbConnection;
            DbCommand.CommandType = commandType;
        }

        public void Open(bool transactionHandling = false)
        {
            if (DbConnection.State != ConnectionState.Open) DbConnection.Open();
            DbCommand.Connection = DbConnection;

            if (transactionHandling)
            {
                Transaction = DbConnection.BeginTransaction();
                DbCommand.Transaction = Transaction;
            }
        }

        public void BeginTransaction()
        {
            Transaction = DbConnection.BeginTransaction();
            DbCommand.Transaction = Transaction;
        }

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            if (Transaction != null) Transaction.Rollback();
        }

        public void Close(bool commitTransaction = false)
        {
            if (commitTransaction) Commit();
            if (DbConnection.State == ConnectionState.Open) DbConnection.Close();
        }

        // set name of procedure or command text --
        public string Command { set => DbCommand.CommandText = value; }

        public string SQLAction
        {
            set { SetParameter("@sql_action", value); }
        }

        public List<DbParam> Parameters
        {
            set
            {
                foreach (var item in value)
                {
                    DbCommand.Parameters.AddWithValue(item.Param, item.Value);
                    if (item.DbType != null) DbCommand.Parameters[item.Param].SqlDbType = (SqlDbType)item.DbType;
                }
            }
        }

        public void SetParameter(string parameterName, object value)
        {
            DbCommand.Parameters.AddWithValue(parameterName, value == null || value.ToString() == "" ? null : value);
        }

        public void AssignParam(string parameterName, object value)
        {
            DbCommand.Parameters[parameterName].Value = value == null || value.ToString() == "" ? null : value;
        }

        public void SetParameterType(string parameterName, SqlDbType dbType)
        {
            DbCommand.Parameters[parameterName].SqlDbType = dbType;
        }

        public void ClearParameters()
        {
            DbCommand.Parameters.Clear();
        }

        public ResultInfo Execute(bool openConnection = true, bool closeConnection = true)
        {
            ResultInfo rInfo = new ResultInfo();
            try
            {
                if (openConnection) Open();
                DbCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                rInfo.SetError(ex.Message, ex.Number, true);
            }
            catch (Exception ex)
            {
                rInfo.SetError(ex.Message, 0, true);
            }
            finally
            {
                if (closeConnection) Close();
            }

            return rInfo;
        }

        public ResultInfo ExecuteScaler(bool openConnection = true, bool closeConnection = true)
        {
            ResultInfo rInfo = new ResultInfo();
            try
            {
                if (openConnection) Open();
                object result = DbCommand.ExecuteScalar();
                if (result == null) rInfo.Result = "";
                else rInfo.Result = result.ToString();
            }
            catch (SqlException ex)
            {
                rInfo.SetError(ex.Message, ex.Number, true);
            }
            catch (Exception ex)
            {
                rInfo.SetError(ex.Message, 0, true);
            }
            finally
            {
                if (closeConnection) Close();
            }

            return rInfo;
        }

        public SqlDataReader ExecuteReader()
        {
            return DbCommand.ExecuteReader();
        }

        public ResultInfo ExecuteTable()
        {
            ResultInfo rInfo = new ResultInfo();

            try
            {
                rInfo.Table = new DataTable();
                SqlDataAdapter adp = new SqlDataAdapter(DbCommand);
                adp.Fill(rInfo.Table);
            }
            catch (SqlException ex)
            {
                rInfo.SetError(ex.Message, ex.Number, true);
            }
            catch (Exception ex)
            {
                rInfo.SetError(ex.Message, 0, true);
            }
            return rInfo;
        }

        public ResultInfo ExecuteDataSet()
        {
            ResultInfo rInfo = new ResultInfo();
            try
            {
                DataSet dataSet = new DataSet();
                SqlDataAdapter adp = new SqlDataAdapter(DbCommand);
                adp.Fill(dataSet);
                rInfo.Object = dataSet;
            }
            catch (SqlException ex)
            {
                rInfo.SetError(ex.Message, ex.Number, true);
            }
            catch (Exception ex)
            {
                rInfo.SetError(ex.Message, 0, true);
            }

            return rInfo;
        }

    }
}