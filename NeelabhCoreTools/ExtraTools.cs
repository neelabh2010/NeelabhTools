using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public static class ExtraTools
{
    public static string IsTrue(this bool value, string trueValue, string falseValue = "")
    {
        return value ? trueValue : falseValue;
    }

    public static string IsFalse(this bool value, string trueValue, string falseValue = "")
    {
        return !value ? trueValue : falseValue;
    }

    public static bool In<T>(this T item, params T[] items)
    {
        if (items == null) return false;
        return items.Contains(item);
    }

    public static T Clone<T>(T source)
    {
        var serialized = JsonConvert.SerializeObject(source);
        return JsonConvert.DeserializeObject<T>(serialized);
    }

    // Table To List --
    public static List<T> TableToList<T>(DataTable dataTable, Func<DataRow, T> dataRowToModelDelegate)
    {
        if (dataTable == null) return null;
        else if (dataRowToModelDelegate == null) return null;

        var list = new List<T>();
        foreach (DataRow dataRow in dataTable.Rows)
            list.Add(dataRowToModelDelegate(dataRow));

        return list;
    }

    public static List<T> TableToList<T>(ResultInfo resultInfo, Func<DataRow, T> dataRowToModelDelegate)
    {
        if (resultInfo.HasError) return null;
        else if (resultInfo.Table == null) return null;
        else if (dataRowToModelDelegate == null) return null;

        var list = new List<T>();
        foreach (DataRow dataRow in resultInfo.Table.Rows)
            list.Add(dataRowToModelDelegate(dataRow));

        return list;
    }

}
