using System;
using System.Text.RegularExpressions;

public static class TypeCheckTools
{
    public static bool IsInt(this string value)
    {
        return int.TryParse(value, out _);
    }

    public static bool IsFloat(this string value)
    {
        return float.TryParse(value, out _);
    }

    public static bool IsDecimal(this string value)
    {
        return decimal.TryParse(value, out _);
    }

    public static bool IsDouble(this string value)
    {
        return double.TryParse(value, out _);
    }

    public static bool IsBool(this string value)
    {
        return bool.TryParse(value, out _);
    }

    public static bool IsDate(this string value)
    {
        return DateTime.TryParse(value, out _);
    }


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


    public static bool IsNullOrEmpty(this string value)
    {
        return value == null || value == string.Empty;
    }

    public static bool IsNotNullOrEmpty(this string value)
    {
        return !(value == null || value == string.Empty);
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

        
    public static bool IsValidEmail(this string email)
    {
        Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        if (regex.Match(email).Success) return true;
        return false;
    }

    public static bool IsValid10DigitsMobile(this string mobile)
    {
        if (mobile == null) return false;
        if (mobile.Length != 10) return false;
        if (mobile.StartsWith("0")) return false;
        if (!ulong.TryParse(mobile, out _)) return false;
        return true;
    }

}
