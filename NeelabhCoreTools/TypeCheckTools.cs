using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Mail;

/// <summary>Type and text checks with compatibility-preserving defaults.</summary>
/// <remarks>
/// Existing methods retain their culture, null, exception and validation behavior. In particular,
/// IsNotNull retains its historical result for missing values; use HasValue for the corrected check.
/// Use IsValidEmailAddress and IsValid10DigitsMobileStrict to opt into the newer validation rules.
/// </remarks>
public static class TypeCheckTools
{
    extension(string value)
    {
        public bool IsInt()
        {
            return int.TryParse(value, out _);
        }

        public bool IsLong()
        {
            return long.TryParse(value, out _);
        }

        public bool IsFloat()
        {
            return float.TryParse(value, out _);
        }

        public bool IsDecimal()
        {
            return decimal.TryParse(value, out _);
        }

        public bool IsDouble()
        {
            return double.TryParse(value, out _);
        }

        public bool IsBool()
        {
            return bool.TryParse(value, out _);
        }

        public bool IsDate()
        {
            return DateTime.TryParse(value, out _);
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(value);
        }

        public bool IsNotNullOrEmpty()
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>True for null, empty text or text made entirely of Unicode whitespace.</summary>
        public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(value);

        /// <summary>True when the string contains a non-whitespace character.</summary>
        public bool IsNotNullOrWhiteSpace() => !string.IsNullOrWhiteSpace(value);

        /// <summary>Parses a date using the supplied culture and styles; null provider means current culture.</summary>
        public bool IsDate(IFormatProvider provider, DateTimeStyles styles = DateTimeStyles.None) =>
            DateTime.TryParse(value, provider, styles, out _);

        /// <summary>
        /// Checks an exact date format. Null provider means invariant culture, matching ToDateExact.
        /// Null or invalid text returns false; invalid style combinations still throw.
        /// </summary>
        public bool IsDateExact(string format = "dd/MM/yyyy", IFormatProvider provider = null,
            DateTimeStyles styles = DateTimeStyles.None) =>
            DateTime.TryParseExact(value, format, provider ?? CultureInfo.InvariantCulture, styles, out _);

        /// <summary>
        /// Checks enum names, numeric values or flag combinations. By default undefined numeric values
        /// are accepted, matching ToEnum. requireDefined restricts results to declared constants;
        /// unnamed combinations of flags are then rejected too.
        /// </summary>
        public bool IsEnum<T>(bool ignoreCase = true, bool requireDefined = false) where T : struct, Enum =>
            Enum.TryParse<T>(value, ignoreCase, out var parsed) && (!requireDefined || Enum.IsDefined(parsed));

        /// <summary>Checks exactly one UTF-16 code unit. Whitespace is valid; surrogate pairs are not a single char.</summary>
        public bool IsChar() => char.TryParse(value, out _);

        /// <summary>Checks a finite Single, rejecting NaN, infinity and values that overflow to infinity.</summary>
        public bool IsFiniteFloat(IFormatProvider provider = null,
            NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
            float.TryParse(value, styles, provider, out var parsed) && float.IsFinite(parsed);

        /// <summary>Checks a finite Double, rejecting NaN, infinity and values that overflow to infinity.</summary>
        public bool IsFiniteDouble(IFormatProvider provider = null,
            NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
            double.TryParse(value, styles, provider, out var parsed) && double.IsFinite(parsed);

        // Parsing checks share the conversion methods' culture and NumberStyles defaults.
        // Invalid text returns false; invalid style combinations still throw.

        /// <summary>Checks int with explicit number styles; null provider means current culture.</summary>
        public bool IsInt(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            int.TryParse(value, styles, provider, out _);

        /// <summary>Checks long with explicit number styles; null provider means current culture.</summary>
        public bool IsLong(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            long.TryParse(value, styles, provider, out _);

        /// <summary>Checks float with explicit number styles; null provider means current culture.</summary>
        public bool IsFloat(IFormatProvider provider, NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
            float.TryParse(value, styles, provider, out _);

        /// <summary>Checks double with explicit number styles; null provider means current culture.</summary>
        public bool IsDouble(IFormatProvider provider, NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
            double.TryParse(value, styles, provider, out _);

        /// <summary>Checks decimal with explicit number styles; null provider means current culture.</summary>
        public bool IsDecimal(IFormatProvider provider, NumberStyles styles = NumberStyles.Number) =>
            decimal.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits byte using the current culture.</summary>
        public bool IsByte() => byte.TryParse(value, out _);

        /// <summary>Checks byte with explicit number styles; null provider means current culture.</summary>
        public bool IsByte(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            byte.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits sbyte using the current culture.</summary>
        public bool IsSByte() => sbyte.TryParse(value, out _);

        /// <summary>Checks sbyte with explicit number styles; null provider means current culture.</summary>
        public bool IsSByte(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            sbyte.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits short using the current culture.</summary>
        public bool IsShort() => short.TryParse(value, out _);

        /// <summary>Checks short with explicit number styles; null provider means current culture.</summary>
        public bool IsShort(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            short.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits ushort using the current culture.</summary>
        public bool IsUShort() => ushort.TryParse(value, out _);

        /// <summary>Checks ushort with explicit number styles; null provider means current culture.</summary>
        public bool IsUShort(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            ushort.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits uint using the current culture.</summary>
        public bool IsUInt() => uint.TryParse(value, out _);

        /// <summary>Checks uint with explicit number styles; null provider means current culture.</summary>
        public bool IsUInt(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            uint.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits ulong using the current culture.</summary>
        public bool IsULong() => ulong.TryParse(value, out _);

        /// <summary>Checks ulong with explicit number styles; null provider means current culture.</summary>
        public bool IsULong(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            ulong.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits nint using the current culture.</summary>
        public bool IsIntPtr() => nint.TryParse(value, out _);

        /// <summary>Checks nint with explicit number styles; null provider means current culture.</summary>
        public bool IsIntPtr(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            nint.TryParse(value, styles, provider, out _);

        /// <summary>Checks whether the text fits nuint using the current culture.</summary>
        public bool IsUIntPtr() => nuint.TryParse(value, out _);

        /// <summary>Checks nuint with explicit number styles; null provider means current culture.</summary>
        public bool IsUIntPtr(IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
            nuint.TryParse(value, styles, provider, out _);
    }

    extension(object target)
    {
        public bool IsNull()
        {
            return target == null || Convert.IsDBNull(target);
        }

        public string IsNull(string trueValue)
        {
            return target.IsNull() ? trueValue : target.ToString();
        }

        public string IsNull(string trueValue, string falseValue)
        {
            return target.IsNull() ? trueValue : falseValue;
        }

        /// <summary>
        /// Legacy predicate whose historical OR expression also returns true for null and DBNull.
        /// Retained for compatibility. Use HasValue to test that a value is present.
        /// </summary>
        public bool IsNotNull()
        {
            return target != null || !Convert.IsDBNull(target);
        }

        /// <summary>Uses the legacy IsNotNull predicate. Use HasValue for corrected missing-value behavior.</summary>
        public string IsNotNull(string trueValue)
        {
            return target.IsNotNull() ? trueValue : target.ToString();
        }

        /// <summary>Uses the legacy IsNotNull predicate. Use HasValue for corrected missing-value behavior.</summary>
        public string IsNotNull(string trueValue, string falseValue)
        {
            return target.IsNotNull() ? trueValue : falseValue;
        }

        public bool IsEmpty()
        {
            return target.ToString() == string.Empty;
        }

        public string IsEmpty(string trueValue)
        {
            return target.ToString() == string.Empty ? trueValue : target.ToString();
        }

        public string IsEmpty(string trueValue, string falseValue)
        {
            return target.ToString() == string.Empty ? trueValue : falseValue;
        }

        public bool IsNotEmpty()
        {
            return target.ToString() != string.Empty;
        }

        public string IsNotEmpty(string trueValue)
        {
            return target.ToString() != string.Empty ? trueValue : target.ToString();
        }

        public string IsNotEmpty(string trueValue, string falseValue)
        {
            return target.ToString() != string.Empty ? trueValue : falseValue;
        }

        public bool IsEqual(string compareWith)
        {
            return target.ToString() == compareWith;
        }
        public string IsEqual(string compareWith, string trueValue, string falseValue = "")
        {
            return target.ToString() == compareWith ? trueValue : falseValue;
        }

        public bool IsNotEqual(string compareWith)
        {
            return target.ToString() != compareWith;
        }

        public string IsNotEqual(string compareWith, string trueValue, string falseValue = "")
        {
            return target.ToString() != compareWith ? trueValue : falseValue;
        }

        /// <summary>True when the target is neither null nor a database-null value. Empty text is a value.</summary>
        public bool HasValue() => !target.IsNull();

        /// <summary>Selects a string using the corrected null/DBNull predicate. Missing values use falseValue.</summary>
        public string HasValue(string trueValue, string falseValue = "") =>
            target.HasValue() ? trueValue : falseValue;

        /// <summary>
        /// Compares text without throwing for null/DBNull. Both missing values are represented as null;
        /// empty text remains distinct. Defaults to ordinal comparison. Custom ToString exceptions propagate.
        /// </summary>
        public bool TextEquals(string compareWith, StringComparison comparison = StringComparison.Ordinal) =>
            string.Equals(GetOptionalText(target), compareWith, comparison);

        /// <summary>The inverse of TextEquals, with the same null and comparison rules.</summary>
        public bool TextNotEquals(string compareWith, StringComparison comparison = StringComparison.Ordinal) =>
            !target.TextEquals(compareWith, comparison);
    }

    extension(object value)
    {
        public bool IsNullOrEmpty()
        {
            return value == null || value.ToString() == string.Empty;
        }

        public bool IsNotNullOrEmpty()
        {
            return !(value == null || value.ToString() == string.Empty);
        }

        /// <summary>Null and DBNull count as missing; other targets are checked using their text representation.</summary>
        public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(GetOptionalText(value));

        /// <summary>True for a nonmissing target whose text contains a non-whitespace character.</summary>
        public bool IsNotNullOrWhiteSpace() => !value.IsNullOrWhiteSpace();
    }

    extension(string email)
    {
        /// <summary>
        /// Preserves the published email pattern, including its limited domain suffix lengths and
        /// exception for null input. Use IsValidEmailAddress for a null-safe mailbox syntax check.
        /// </summary>
        public bool IsValidEmail()
        {
            // The static API caches the regex and avoids allocating a Regex and Match for each call.
            return Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        }

        /// <summary>
        /// Checks a single bare mailbox address using the .NET mail parser. Supports plus addressing
        /// and longer domain suffixes. Rejects display names, control characters and surrounding whitespace.
        /// This checks syntax only, not domain existence, mailbox delivery or SMTP server support.
        /// </summary>
        public bool IsValidEmailAddress()
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            foreach (char character in email)
                if (char.IsControl(character)) return false;
            return MailAddress.TryCreate(email, out var address) &&
                string.Equals(address.Address, email, StringComparison.Ordinal);
        }
    }

    extension(string mobile)
    {
        /// <summary>
        /// Preserves the legacy length, prefix and numeric-parsing checks (including sign/whitespace
        /// acceptance). Use IsValid10DigitsMobileStrict to require exactly ten ASCII digits.
        /// </summary>
        public bool IsValid10DigitsMobile()
        {
            if (mobile == null) return false;
            if (mobile.Length != 10) return false;
            if (mobile.StartsWith("0")) return false;
            if (!ulong.TryParse(mobile, out _)) return false;
            return true;
        }

        /// <summary>
        /// Requires exactly ten ASCII digits and a nonzero first digit. Does not validate a country's
        /// mobile-number allocations, carrier, or whether the number is in use.
        /// </summary>
        public bool IsValid10DigitsMobileStrict()
        {
            if (mobile == null || mobile.Length != 10 || mobile[0] == '0') return false;
            foreach (char digit in mobile)
                if (digit < '0' || digit > '9') return false;
            return true;
        }
    }

    private static string GetOptionalText(object value) => value.IsNull() ? null : value.ToString();

}
