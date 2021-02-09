using System;
using System.Text.RegularExpressions;
using System.Globalization;

public static class StringTools
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

    public static bool IsNullOrEmpty(this string value)
    {
        return value == null || value == string.Empty;
    }

    public static bool IsNotNullOrEmpty(this string value)
    {
        return !(value == null || value == string.Empty);
    }

    public static string ReplaceOccurrance(this string target, string replaceTo, string replaceWith, int occurrance = 1)
    {
        var regex = new Regex(Regex.Escape(replaceTo));
        var newText = regex.Replace(target, replaceWith, occurrance);
        return newText;
    }

    public static string TrimStart(this string target, string trimString = " ")
    {
        if (string.IsNullOrEmpty(trimString)) return target;

        string result = target;
        if (result.StartsWith(trimString))
        {
            result = result.Substring(trimString.Length);
        }

        return result;
    }

    public static string TrimEnd(this string target, string trimString = " ")
    {
        if (string.IsNullOrEmpty(trimString)) return target;

        string result = target;
        if (result.EndsWith(trimString))
        {
            result = result.Substring(0, result.Length - trimString.Length);
        }

        return result;
    }

    public static string[] Split(this string target, string splitBy = "##", StringSplitOptions splitOption = StringSplitOptions.RemoveEmptyEntries)
    {
        return target.Split(new string[] { splitBy }, splitOption);
    }

    public static string ToTitleCase(this string target)
    {
        TextInfo textInfo = new CultureInfo("en-us", false).TextInfo;
        return textInfo.ToTitleCase(target);
    }

    public static string ConcatIf(this string target, string compareWith, string addString)
    {
        return target == compareWith ? target += addString : target;
    }

    public static string ConcatIfNot(this string target, string compareWith, string addString)
    {
        return target != compareWith ? target += addString : target;
    }

    public static string Mid(this string target, string preString, string postString)
    {
        int start_idx, end_Idx;

        start_idx = target.IndexOf(preString);
        start_idx += preString.Length;

        end_Idx = target.IndexOf(postString, start_idx);
        end_Idx -= 1;

        return target.Substring(start_idx, end_Idx - start_idx + 1);
    }

    public static string FormatDate(this string target, string format = "dd-MMM-yyyy")
    {
        return DateTime.Parse(target).ToString(format);
    }
}
