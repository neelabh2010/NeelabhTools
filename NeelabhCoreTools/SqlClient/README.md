# DbTools compatibility and safer usage

Existing callers can continue using the published constructor, methods, properties, and `DbParam` fields. `ExecuteScaler` remains an alias without an obsolete warning. The existing methods still return `ResultInfo`, and scalar values still populate its string `Result` property. The package version and runtime dependencies are unchanged.

`UseSafeBehavior` defaults to `false`. Enable it per instance after reviewing these differences:

| Operation | Existing default | With `UseSafeBehavior = true` |
| --- | --- | --- |
| `Execute` and scalar execution with `closeConnection: true` | Close the connection even if the caller opened it or a transaction is active | Close only a connection opened by that operation, with no active transaction |
| `AssignParam` | Empty text, C# null, and `DBNull.Value` become an unset parameter value | Preserve empty text and convert C# null to `DBNull.Value` |
| `Parameters` and `SQLAction` assignment | Append parameters | Update existing names, preserving unrelated parameters and explicit metadata |
| Successful `Commit` or `Rollback` | Retain the transaction reference | Clear the helper and command references and dispose the completed transaction |
| `Close(true)` when commit fails | Preserve the original exception and legacy connection behavior | Close and clean up in `finally` |

Legacy callers managing a transaction must continue passing `closeConnection: false` to execution methods until they enable safe behavior. New typed and async execution methods follow the same instance setting and connection flags. In safe mode, `closeConnection: true` permits automatic cleanup; use an explicit `Close()` to close a caller-opened connection. `Close()` rolls back an unfinished transaction. Choose safe behavior before starting work and keep it consistent throughout a transaction.

```csharp
using var db = new DbTools(connectionString) { UseSafeBehavior = true };
db.Open(transactionHandling: true);
db.Command = "dbo.SaveCustomer";
db.AddParameter(new SqlParameter("@Name", SqlDbType.NVarChar, 100)
{
    Value = customerName
});
ResultInfo result = db.Execute();
if (result.HasError)
    throw new InvalidOperationException(result.SystemMessage, db.LastException);

db.Commit(); // An earlier exception leaves disposal to roll back the unfinished transaction.
```

An operation failure does not automatically commit or roll back a transaction. The caller decides whether to roll back; rollback itself can fail after a connection failure. Disposal rolls back an unfinished transaction. Handle rollback failures separately when preserving the original exception is required.

## Parameters and typed results

The new parameter methods use SQL null normalization in either mode:

- `AddParameter(DbParam)` adds a parameter and rejects duplicate names.
- `AddParameter(SqlParameter)` accepts `SqlDbType`, `Size`, `Precision`, `Scale`, `Direction`, and `TypeName` without changing the published `DbParam` structure. Output values remain available through the parameter after execution (after reader disposal for reader operations).
- `SetParameterValue(name, value)` updates an existing parameter, preserves empty strings, and maps C# null to SQL `NULL`.
- `SetParameters(IEnumerable<DbParam>)` replaces the complete set after validating it. This also removes any previously assigned `SQLAction` parameter.

Prefer explicit types and metadata matching the database schema. Omit an optional stored-procedure parameter to use its server default; assigning `DBNull.Value` requests SQL `NULL` instead.

`ExecuteNonQuery()` returns `DbResult<int>` with the provider's affected-row count. A successful count can be `-1`, for example with `SET NOCOUNT ON`.

`ExecuteScalar<T>()` and `ExecuteScalarAsync<T>()` return `DbResult<T>`:

| Outcome | `HasError` | `HasValue` | `IsDbNull` |
| --- | --- | --- | --- |
| Value, including an empty string | false | true | false |
| SQL `NULL` | false | false | true |
| No row | false | false | false |
| Execution or conversion failure | true | Check `HasError` first | Check `HasError` first |

Exact CLR types are preserved; other convertible types use invariant-culture conversion. Failed conversions are returned as errors. `Value` is `default(T)` when no value is present. `DbResult<T>.Exception` and `DbTools.LastException` retain original exception details for diagnostics. Avoid serializing these exception objects directly as API responses. Legacy `ResultInfo` has not acquired new properties.

## Async operations and resource ownership

Async methods are available for opening, transaction control, connectivity tests, non-query execution, scalar execution, and readers. They accept `CancellationToken`; cancellation propagates as `OperationCanceledException`. A pre-cancelled call does not open or close a connection. `CommandTimeout` forwards to the underlying command timeout, in seconds.

```csharp
using var db = new DbTools(connectionString, CommandType.Text)
{
    UseSafeBehavior = true,
    CommandTimeout = 30,
    Command = "SELECT COUNT(*) FROM dbo.Customers"
};
DbResult<int> count = await db.ExecuteScalarAsync<int>(cancellationToken: cancellationToken);
if (count.HasError)
    throw new InvalidOperationException(count.SystemMessage, count.Exception);
```

`ExecuteReader` and `ExecuteReaderAsync` require an open connection and transfer reader disposal to the caller. Passing `CommandBehavior.CloseConnection` explicitly makes reader disposal close the connection, even in safe mode. `ExecuteTable` and `ExecuteDataSet` remain synchronous and preserve adapter connection behavior; their adapters are now disposed locally.

`TestConnection` now uses a cloned, temporary connection, including when the working connection is already open. It does not close the working connection or end its transaction.

`DbTools` implements `IDisposable`; use `using` for resources owned by the helper. Disposal releases the currently assigned connection, command, and transaction. Public setters remain available for compatibility. Replacing a resource does not dispose the previous object; its owner remains responsible for it. Do not dispose a helper while its assigned resources are in use elsewhere. Safe mode validates connection and transaction consistency before execution. Instances are not safe for concurrent operations.

## Running regression tests

Run `dotnet test NeelabhCoreTools.Tests/NeelabhCoreTools.Tests.csproj` from the repository root. SQL Server tests are skipped unless `DBTOOLS_TEST_CONNECTION_STRING` is set. They use literals, table variables, and connection-local temporary objects, with no persistent schema changes.

Example for a local SQL Server with Windows authentication, in PowerShell:

```powershell
$env:DBTOOLS_TEST_CONNECTION_STRING = 'Server=localhost;Database=tempdb;Integrated Security=true;TrustServerCertificate=true'
dotnet test NeelabhCoreTools.Tests/NeelabhCoreTools.Tests.csproj
```

The certificate override in this example is for local development only; configure certificate validation for deployed environments.
