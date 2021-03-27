using System;

public static class ObjectTools
{
    public static bool ToBool(this object target)
    {
        return bool.Parse(target.ToString().ToLower());
    }

    public static short ToShort(this object target)
    {
        return short.Parse(target.ToString());
    }

    public static int ToInt(this object target)
    {
        return int.Parse(target.ToString());
    }

    public static long ToLong(this object target)
    {
        return long.Parse(target.ToString());
    }

    public static float ToFloat(this object target)
    {
        return float.Parse(target.ToString());
    }

    public static double ToDouble(this object target)
    {
        return double.Parse(target.ToString());
    }

    public static decimal ToDecimal(this object target)
    {
        return decimal.Parse(target.ToString());
    }

    // convert string to enum target --
    public static T ToEnum<T>(this object target)
    {
        return (T)Enum.Parse(typeof(T), target.ToString(), true);
    }

    //For Nullable DataTypes
    public static bool? ToNBool(this object target)  // nullable bool
    {
        return target.IsNull() ? null : (bool?)bool.Parse(target.ToString());
    }

    public static int? ToNInt(this object target) // nullable int
    {
        return target.IsNull() ? null :(int?) int.Parse(target.ToString());
    }

    public static long? ToNLong(this object target) // nullable long
    {
        return target.IsNull() ? null : (long?)long.Parse(target.ToString());
    }

    public static decimal? ToNDecimal(this object target) // nullable decimal
    {
        return target.IsNull() ? null : (decimal?)decimal.Parse(target.ToString());
    }

    public static double? ToNDouble(this object target) // nullable double
    {
        return target.IsNull() ? null : (double?)double.Parse(target.ToString());
    }
    
    //-- For Date --
    public static DateTime ToDate(this object target)
    {
        return DateTime.Parse(target.ToString());
    }

    public static DateTime? ToNDate(this object target) // nullable DateTime
    {
        return target.IsNull() ? null : (DateTime?)DateTime.Parse(target.ToString());
    }

    public static string FormatDate(this object target, string foramt = "dd-MMM-yyyy")
    {
        return DateTime.Parse(target.ToString()).ToString(foramt);
    }

    public static string ReplaceIF(this object target, string compareWith, string trueValue)
    {
        return target.ToString() == compareWith ? trueValue : target.ToString();
    }

    public static string ReplaceIF(this object target, string compareWith, string trueValue, string falseValue)
    {
        return target.ToString() == compareWith ? trueValue : falseValue;
    }

    // Validation --
    public static bool IsNull(this object target)
    {
        return target == null || Convert.IsDBNull(target);
    }

    public static string IsNull(this object target, string trueValue)
    {
        return target.IsNull() ? trueValue : target.ToString();
    }

    public static string IsNull(this object target, string trueValue, string falseValue)
    {
        return target.IsNull() ? trueValue : falseValue;
    }

    public static bool IsNotNull(this object target)
    {
        return target != null || !Convert.IsDBNull(target);
    }

    public static string IsNotNull(this object target, string trueValue)
    {
        return target.IsNotNull() ? trueValue : target.ToString();
    }

    public static string IsNotNull(this object target, string trueValue, string falseValue)
    {
        return target.IsNotNull() ? trueValue : falseValue;
    }

    public static bool IsEqual(this object target, string compareWith)
    {
        return target.ToString() == compareWith;
    }
    public static string IsEqual(this object target, string compareWith, string trueValue, string falseValue = "")
    {
        return target.ToString() == compareWith ? trueValue : falseValue;
    }

    public static bool IsNotEqual(this object target, string compareWith)
    {
        return target.ToString() != compareWith;
    }

    public static string IsNotEqual(this object target, string compareWith, string trueValue, string falseValue = "")
    {
        return target.ToString() != compareWith ? trueValue : falseValue;
    }

    public static bool IsEmpty(this object target)
    {
        return target.ToString() == string.Empty;
    }

    public static string IsEmpty(this object target, string trueValue)
    {
        return target.ToString() == string.Empty ? trueValue : target.ToString();
    }

    public static string IsEmpty(this object target, string trueValue, string falseValue)
    {
        return target.ToString() == string.Empty ? trueValue : falseValue;
    }

    public static bool IsNotEmpty(this object target)
    {
        return target.ToString() != string.Empty;
    }

    public static string IsNotEmpty(this object target, string trueValue)
    {
        return target.ToString() != string.Empty ? trueValue : target.ToString();
    }

    public static string IsNotEmpty(this object target, string trueValue, string falseValue)
    {
        return target.ToString() != string.Empty ? trueValue : falseValue;
    }

}
