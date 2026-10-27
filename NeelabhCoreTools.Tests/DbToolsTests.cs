using System.Data;
using Microsoft.Data.SqlClient;
using NeelabhCoreTools.SqlClient;

namespace NeelabhCoreTools.Tests;

public class DbToolsTests
{
    [Fact]
    public void PublishedDefaultsAndDbParamFieldsAreUnchanged()
    {
        using var db = new DbTools();
        Assert.False(db.UseSafeBehavior);
        Assert.Equal("", db.DbConnection.ConnectionString);
        Assert.Equal(CommandType.StoredProcedure, db.DbCommand.CommandType);
        Assert.Same(db.DbConnection, db.DbCommand.Connection);
        Assert.Null(db.Transaction);
        Assert.Equal(new[] { "DbType", "Param", "Value" },
            typeof(DbParam).GetFields().Select(field => field.Name).Order().ToArray());
        foreach (var name in new[] { "Execute", "ExecuteScalar", "ExecuteScaler" })
        {
            var method = typeof(DbTools).GetMethods().Single(m => m.Name == name && !m.IsGenericMethod);
            Assert.Equal(typeof(ResultInfo), method.ReturnType);
            Assert.All(method.GetParameters(), p => Assert.Equal(true, p.DefaultValue));
        }
        foreach (var name in new[] { "DbConnection", "DbCommand", "Transaction" })
            Assert.True(typeof(DbTools).GetProperty(name)!.SetMethod!.IsPublic);
    }

    [Fact]
    public void LegacyParameterSettersStillAppendAndAssignParamRetainsUnsetSemantics()
    {
        using var db = new DbTools();
        db.SQLAction = "insert";
        db.SQLAction = "update";
        db.Parameters = [new DbParam { Param = "@name", Value = "first" }];
        db.Parameters = [new DbParam { Param = "@name", Value = "second" }];
        Assert.Equal(4, db.DbCommand.Parameters.Count);
        db.AssignParam("@name", "");
        Assert.Null(db.DbCommand.Parameters["@name"].Value);
        db.AssignParam("@name", DBNull.Value);
        Assert.Null(db.DbCommand.Parameters["@name"].Value);
    }

    [Fact]
    public void SafeParameterSettersUpsertAndKeepActionAndMetadata()
    {
        using var db = new DbTools { UseSafeBehavior = true };
        db.SQLAction = "insert";
        db.SQLAction = "update";
        db.AddParameter(new SqlParameter("@name", SqlDbType.VarChar, 50) { Value = "first" });
        db.Parameters = [new DbParam { Param = "name", Value = "second" }];
        Assert.Equal(2, db.DbCommand.Parameters.Count);
        Assert.Equal("update", db.DbCommand.Parameters["sql_action"].Value);
        var parameter = db.DbCommand.Parameters["@name"];
        Assert.Equal("second", parameter.Value);
        Assert.Equal(SqlDbType.VarChar, parameter.SqlDbType);
        Assert.Equal(50, parameter.Size);
        db.AssignParam("@name", "");
        Assert.Equal("", parameter.Value);
        db.AssignParam("@name", null);
        Assert.Same(DBNull.Value, parameter.Value);
        db.AssignParam("@name", DBNull.Value);
        Assert.Same(DBNull.Value, parameter.Value);
    }

    [Fact]
    public void NewParameterApisNormalizeNullsEvenInLegacyMode()
    {
        using var db = new DbTools();
        var parameter = db.AddParameter(new DbParam { Param = "@value", DbType = SqlDbType.Int });
        Assert.Same(DBNull.Value, parameter.Value);
        db.SetParameterValue("@value", "");
        Assert.Equal("", parameter.Value);
        db.SetParameterValue("@value", null);
        Assert.Same(DBNull.Value, parameter.Value);
        Assert.Throws<ArgumentException>(() => db.AddParameter(new SqlParameter("value", 1)));
        db.SetParameters([new DbParam { Param = "@new", Value = 2 }]);
        Assert.Equal(1, db.DbCommand.Parameters.Count);
        Assert.Equal(2, db.DbCommand.Parameters["@new"].Value);
    }

    [Fact]
    public void InvalidReplacementParametersDoNotDiscardExistingParameters()
    {
        using var db = new DbTools();
        db.AddParameter(new DbParam { Param = "@existing", Value = 1 });
        Assert.Throws<ArgumentException>(() => db.SetParameters([
            new DbParam { Param = "@duplicate", Value = 2 },
            new DbParam { Param = "duplicate", Value = 3 }]));
        Assert.Equal(1, db.DbCommand.Parameters.Count);
        Assert.Equal(1, db.DbCommand.Parameters["@existing"].Value);
    }

    [Fact]
    public void FullSqlParameterMetadataIsRetained()
    {
        using var db = new DbTools();
        var parameter = new SqlParameter("@amount", SqlDbType.Decimal)
        {
            Precision = 12, Scale = 2, Direction = ParameterDirection.InputOutput, Value = 12.34m
        };
        Assert.Same(parameter, db.AddParameter(parameter));
        Assert.Equal((byte)12, parameter.Precision);
        Assert.Equal((byte)2, parameter.Scale);
        Assert.Equal(ParameterDirection.InputOutput, parameter.Direction);
        var table = db.AddParameter(new SqlParameter("@rows", SqlDbType.Structured)
        { TypeName = "dbo.TestRows", Value = new DataTable() });
        Assert.Equal("dbo.TestRows", table.TypeName);
        db.CommandTimeout = 17;
        Assert.Equal(17, db.DbCommand.CommandTimeout);
    }

    [Fact]
    public void FailureRetainsOriginalExceptionAndLegacyResultType()
    {
        using var db = new DbTools { UseSafeBehavior = true };
        var result = db.Execute();
        Assert.IsType<ResultInfo>(result);
        Assert.True(result.HasError);
        Assert.IsType<InvalidOperationException>(db.LastException);
        var typed = db.ExecuteScalar<int>();
        Assert.True(typed.HasError);
        Assert.Same(db.LastException, typed.Exception);
        Assert.False(typed.HasValue);
    }

    [Fact]
    public void DisposalIsIdempotentAndFurtherOperationsFailClearly()
    {
        var db = new DbTools();
        db.Dispose();
        db.Dispose();
        Assert.Throws<ObjectDisposedException>(() => db.Open());
        Assert.True(db.Execute().HasError);
        Assert.IsType<ObjectDisposedException>(db.LastException);
    }

    [Fact]
    public async Task PreCancelledAsyncOperationsPropagateCancellation()
    {
        using var db = new DbTools();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            db.ExecuteAsync(cancellationToken: cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            db.TestConnectionAsync(cancellation.Token));
        Assert.Equal(ConnectionState.Closed, db.DbConnection.State);
    }
}
