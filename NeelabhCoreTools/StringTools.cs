using System;
using System.Globalization;
using System.Text.RegularExpressions;

public static class StringTools
{
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

    public static string ReplaceIF(this string target, string compareWith, string trueValue)
    {
        return target.ToString() == compareWith ? trueValue : target.ToString();
    }

    public static string ReplaceIF(this string target, string compareWith, string trueValue, string falseValue)
    {
        return target.ToString() == compareWith ? trueValue : falseValue;
    }

    public static string ReplaceOccurrance(this string target, string replaceTo, string replaceWith, int occurrance = 1)
    {
        var regex = new Regex(Regex.Escape(replaceTo));
        var newText = regex.Replace(target, replaceWith, occurrance);
        return newText;
    }

    public static int CountOccurrance(this string target, string searchString)
    {
        return Regex.Matches(target, searchString).Count;
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

    public static string ConcatIf(this string target, bool whenTrue, string addString)
    {
        return whenTrue ? target += addString : target;
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

    public static string Shuffle(this string target)
    {
        char[] array = target.ToCharArray();
        Random rng = new Random();
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
        return new string(array);
    }
}
