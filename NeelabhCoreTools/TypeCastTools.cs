using System;
using System.Globalization;

public static class TypeCastTools
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

    //For Nullable Data Types
    public static bool? ToNBool(this object target)  // nullable bool
    {
        return target.IsNull() ? null : (bool?)bool.Parse(target.ToString());
    }

    public static int? ToNInt(this object target) // nullable int
    {
        return target.IsNull() ? null : (int?)int.Parse(target.ToString());
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

    public static DateTime ToDateExact(this object target, string format = "dd/MM/yyyy")
    {
        return DateTime.ParseExact(target.ToString(), format, CultureInfo.InvariantCulture);
    }

    public static DateTime? ToNDate(this object target) // nullable DateTime
    {
        return target.IsNull() ? null : (DateTime?)DateTime.Parse(target.ToString());
    }

    public static DateTime? ToNDateExact(this object target, string format = "dd/MM/yyyy")
    {
        return target.IsNull() ? null : (DateTime?) DateTime.ParseExact(target.ToString(), format, CultureInfo.InvariantCulture);
    }

    public static string FormatDate(this object target, string foramt = "dd-MMM-yyyy")
    {
        return DateTime.Parse(target.ToString()).ToString(foramt);
    }
}
