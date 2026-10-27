using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public static class ExtraTools
{
    extension(bool value)
    {
        /// <summary>Returns trueValue when value is true; otherwise returns falseValue.</summary>
        public string IsTrue(string trueValue, string falseValue = "")
        {
            return value ? trueValue : falseValue;
        }

        /// <summary>
        /// Returns trueValue when value is false; otherwise returns falseValue.
        /// Parameter names are retained for compatibility with existing named arguments.
        /// </summary>
        public string IsFalse(string trueValue, string falseValue = "")
        {
            return !value ? trueValue : falseValue;
        }
    }

    extension<T>(T item)
    {
        /// <summary>
        /// Tests membership using the default equality comparer. A null or empty candidate array
        /// returns false; a null item can match a null candidate.
        /// </summary>
        public bool In(params T[] items)
        {
            if (items == null) return false;
            return items.Contains(item);
        }

        /// <summary>
        /// Tests membership in a collection or lazy sequence without materializing it.
        /// A null sequence returns false. A null comparer uses EqualityComparer&lt;T&gt;.Default,
        /// regardless of any comparer configured on the collection itself.
        /// </summary>
        /// <remarks>
        /// This has a distinct name so existing In calls that treat a collection as one candidate
        /// keep their original meaning. Enumeration stops at the first match.
        /// </remarks>
        public bool InCollection(IEnumerable<T> items, IEqualityComparer<T> comparer = null)
        {
            return items != null && items.Contains(item, comparer);
        }
    }

    /// <summary>Copies the JSON-serializable representation of source using Json.NET defaults.</summary>
    /// <remarks>
    /// This preserves the existing JsonConvert.DefaultSettings behavior. Members ignored by JSON
    /// serialization are not copied; graph identity, cycles and derived types follow the serializer's
    /// settings. Serialization and deserialization failures are propagated to the caller.
    /// </remarks>
    public static T Clone<T>(T source) => Clone(source, null);

    /// <summary>
    /// Copies the JSON-serializable representation of source with the supplied settings applied
    /// to both serialization and deserialization. Null settings retain Json.NET defaults.
    /// </summary>
    public static T Clone<T>(T source, JsonSerializerSettings settings)
    {
        var serialized = JsonConvert.SerializeObject(source, settings);
        return JsonConvert.DeserializeObject<T>(serialized, settings);
    }

    /// <summary>
    /// Tries to clone the JSON representation. JSON failures return false, default(T) and the
    /// original exception. Other exceptions (such as errors in custom converters) propagate.
    /// A successful clone may be null.
    /// </summary>
    public static bool TryClone<T>(T source, out T clone, out JsonException error,
        JsonSerializerSettings settings = null)
    {
        try
        {
            clone = Clone(source, settings);
            error = null;
            return true;
        }
        catch (JsonException ex)
        {
            clone = default;
            error = ex;
            return false;
        }
    }

    /// <summary>
    /// Maps rows in their existing order. A missing table or mapper returns null for compatibility;
    /// an empty table returns an empty list. Mapper exceptions propagate without returning partial data.
    /// </summary>
    public static List<T> TableToList<T>(DataTable dataTable, Func<DataRow, T> dataRowToModelDelegate)
    {
        if (dataTable == null) return null;
        else if (dataRowToModelDelegate == null) return null;

        var list = new List<T>(dataTable.Rows.Count);
        foreach (DataRow dataRow in dataTable.Rows)
            list.Add(dataRowToModelDelegate(dataRow));

        return list;
    }

    /// <summary>
    /// Maps a successful result's table. A missing result, failed result, missing table or missing
    /// mapper returns null. The mapper is not invoked for these cases.
    /// </summary>
    public static List<T> TableToList<T>(ResultInfo resultInfo, Func<DataRow, T> dataRowToModelDelegate)
    {
        if (resultInfo == null || resultInfo.HasError) return null;
        return TableToList(resultInfo.Table, dataRowToModelDelegate);
    }

}
