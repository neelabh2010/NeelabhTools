using System;

namespace NeelabhCoreTools.SqlClient
{
    internal interface IDbResult
    {
        void SetException(Exception exception);
    }

    /// <summary>A typed database result. Always check HasError before reading Value.</summary>
    public class DbResult<T> : ResultInfo, IDbResult
    {
        public T Value { get; internal set; }
        public bool HasValue { get; internal set; }
        public bool IsDbNull { get; internal set; }
        public Exception Exception { get; private set; }

        void IDbResult.SetException(Exception exception) => Exception = exception;
    }
}
