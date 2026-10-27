using System;
using System.Globalization;

/// <summary>Text-based conversions with compatibility-preserving parsing defaults.</summary>
/// <remarks>
/// Existing methods retain their current-culture parsing and exception behavior. Nullable methods
/// return null only for null or database-null input; empty or invalid text still throws.
/// Exact-date methods default to invariant culture. Use the TryTo methods for invalid user input,
/// or the provider overloads when a culture must be specified explicitly.
/// </remarks>
public static class TypeCastTools
{
    /// <summary>Parses Boolean text using the original normalization and exception behavior.</summary>
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

    /// <summary>
    /// Parses enum names without case sensitivity. Numeric values and flag combinations remain
    /// supported, including values not declared as named constants. The published generic method
    /// remains unconstrained; passing a non-enum type fails at runtime as before.
    /// </summary>
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
        return target.IsNull() ? null : target.ToInt();
    }

    public static long? ToNLong(this object target) // nullable long
    {
        return target.IsNull() ? null : target.ToLong();
    }

    public static decimal? ToNDecimal(this object target) // nullable decimal
    {
        return target.IsNull() ? null : target.ToDecimal();
    }

    public static double? ToNDouble(this object target) // nullable double
    {
        return target.IsNull() ? null : target.ToDouble();
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
        return target.IsNull() ? null : target.ToDate();
    }

    public static DateTime? ToNDateExact(this object target, string format = "dd/MM/yyyy")
    {
        return target.IsNull() ? null : target.ToDateExact(format);
    }

    /// <summary>
    /// Parses and formats using the current culture. The published parameter name "foramt"
    /// is intentionally retained for named-argument compatibility.
    /// </summary>
    public static string FormatDate(this object target, string foramt = "dd-MMM-yyyy")
    {
        return DateTime.Parse(target.ToString()).ToString(foramt);
    }

    // Provider overloads require an explicit provider argument, leaving existing calls unchanged.

    /// <summary>Parses an Int16 with the supplied culture and number styles.</summary>
    public static short ToShort(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        short.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Parses an Int32 with the supplied culture and number styles.</summary>
    public static int ToInt(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        int.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Parses an Int64 with the supplied culture and number styles.</summary>
    public static long ToLong(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        long.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Parses a Single with the supplied culture, retaining the runtime's NaN/infinity behavior.</summary>
    public static float ToFloat(this object target, IFormatProvider provider,
        NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
        float.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Parses a Double with the supplied culture, retaining the runtime's NaN/infinity behavior.</summary>
    public static double ToDouble(this object target, IFormatProvider provider,
        NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
        double.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Parses a Decimal with the supplied culture and number styles.</summary>
    public static decimal ToDecimal(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Number) =>
        decimal.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; other values are parsed using the supplied culture and styles.</summary>
    public static int? ToNInt(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToInt(provider, styles);

    /// <inheritdoc cref="ToNInt(object, IFormatProvider, NumberStyles)"/>
    public static long? ToNLong(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToLong(provider, styles);

    /// <inheritdoc cref="ToNInt(object, IFormatProvider, NumberStyles)"/>
    public static decimal? ToNDecimal(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Number) =>
        target.IsNull() ? null : target.ToDecimal(provider, styles);

    /// <inheritdoc cref="ToNInt(object, IFormatProvider, NumberStyles)"/>
    public static double? ToNDouble(this object target, IFormatProvider provider,
        NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
        target.IsNull() ? null : target.ToDouble(provider, styles);

    /// <summary>Parses date text with the supplied culture and date styles.</summary>
    public static DateTime ToDate(this object target, IFormatProvider provider, DateTimeStyles styles = DateTimeStyles.None) =>
        DateTime.Parse(GetParseText(target, provider), provider, styles);

    /// <summary>Parses an exact date. A null provider uses invariant culture, matching the original exact-date method.</summary>
    public static DateTime ToDateExact(this object target, string format, IFormatProvider provider,
        DateTimeStyles styles = DateTimeStyles.None)
    {
        provider ??= CultureInfo.InvariantCulture;
        return DateTime.ParseExact(GetParseText(target, provider), format, provider, styles);
    }

    /// <summary>Returns null for null/DBNull; otherwise parses using the supplied culture and styles.</summary>
    public static DateTime? ToNDate(this object target, IFormatProvider provider, DateTimeStyles styles = DateTimeStyles.None) =>
        target.IsNull() ? null : target.ToDate(provider, styles);

    /// <summary>Returns null for null/DBNull; otherwise parses exactly. A null provider uses invariant culture.</summary>
    public static DateTime? ToNDateExact(this object target, string format, IFormatProvider provider,
        DateTimeStyles styles = DateTimeStyles.None) =>
        target.IsNull() ? null : target.ToDateExact(format, provider, styles);

    /// <summary>
    /// Formats using an explicit culture. DateTime inputs preserve their ticks and Kind; other
    /// inputs are first parsed using the same culture. Null provider means the current culture.
    /// </summary>
    public static string FormatDate(this object target, string format, IFormatProvider provider)
    {
        var date = target is DateTime value ? value : target.ToDate(provider);
        return date.ToString(format, provider);
    }

    /// <summary>
    /// Tries to parse Boolean text, returning false for null, DBNull or invalid text. Numeric
    /// zero/one are not Boolean text. Exceptions from a custom ToString implementation propagate.
    /// </summary>
    public static bool TryToBool(this object target, out bool value) =>
        bool.TryParse(target?.ToString(), out value);

    /// <summary>
    /// Tries to parse a number. Null/DBNull, invalid text and overflow return false with a default
    /// output value. Invalid style combinations and custom formatting exceptions still propagate.
    /// A null provider means current culture; IFormattable inputs use the same provider before parsing.
    /// </summary>
    public static bool TryToShort(this object target, out short value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        short.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToInt(this object target, out int value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        int.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToLong(this object target, out long value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        long.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>
    /// Tries to parse a Single with the supplied culture and styles. Null/DBNull and invalid text
    /// return false. NaN, infinity and overflow-to-infinity retain the runtime's successful parsing
    /// behavior; use float.IsFinite when a finite value is required. Invalid styles or custom
    /// formatting errors propagate.
    /// </summary>
    public static bool TryToFloat(this object target, out float value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
        float.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>
    /// Tries to parse a Double with the supplied culture and styles. Null/DBNull and invalid text
    /// return false. NaN, infinity and overflow-to-infinity retain the runtime's successful parsing
    /// behavior; use double.IsFinite when a finite value is required. Invalid styles or custom
    /// formatting errors propagate.
    /// </summary>
    public static bool TryToDouble(this object target, out double value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
        double.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToDecimal(this object target, out decimal value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Number) =>
        decimal.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>
    /// Tries to parse enum names, numeric values and flag combinations. Undefined numeric values
    /// remain accepted; use Enum.IsDefined separately when named-value validation is required.
    /// </summary>
    public static bool TryToEnum<T>(this object target, out T value, bool ignoreCase = true) where T : struct, Enum =>
        Enum.TryParse(target?.ToString(), ignoreCase, out value);

    /// <summary>
    /// Tries to parse a date using the supplied culture and styles. Null/DBNull or invalid input
    /// returns false. Invalid styles or custom formatting errors propagate.
    /// </summary>
    public static bool TryToDate(this object target, out DateTime value, IFormatProvider provider = null,
        DateTimeStyles styles = DateTimeStyles.None) =>
        DateTime.TryParse(GetParseText(target, provider), provider, styles, out value);

    /// <summary>
    /// Tries to parse an exact date. Null provider means invariant culture, matching ToDateExact.
    /// Null/DBNull or invalid input returns false. Invalid styles or custom formatting errors propagate.
    /// </summary>
    public static bool TryToDateExact(this object target, out DateTime value, string format = "dd/MM/yyyy",
        IFormatProvider provider = null, DateTimeStyles styles = DateTimeStyles.None)
    {
        provider ??= CultureInfo.InvariantCulture;
        return DateTime.TryParseExact(GetParseText(target, provider), format, provider, styles, out value);
    }

    // Remaining built-in integer types. IntPtr/UIntPtr names distinguish native integers
    // from the existing ToNInt method, which returns a nullable Int32.

    /// <summary>Parses a Byte using the current culture.</summary>
    public static byte ToByte(this object target) => byte.Parse(target.ToString());

    /// <summary>Parses a Byte with the supplied culture and number styles.</summary>
    public static byte ToByte(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        byte.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; otherwise parses a Byte using the current culture.</summary>
    public static byte? ToNByte(this object target) => target.IsNull() ? null : target.ToByte();

    /// <summary>Returns null for null/DBNull; otherwise parses with the supplied culture and number styles.</summary>
    public static byte? ToNByte(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToByte(provider, styles);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToByte(this object target, out byte value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        byte.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>Parses a SByte using the current culture.</summary>
    public static sbyte ToSByte(this object target) => sbyte.Parse(target.ToString());

    /// <summary>Parses a SByte with the supplied culture and number styles.</summary>
    public static sbyte ToSByte(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        sbyte.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; otherwise parses a SByte using the current culture.</summary>
    public static sbyte? ToNSByte(this object target) => target.IsNull() ? null : target.ToSByte();

    /// <summary>Returns null for null/DBNull; otherwise parses with the supplied culture and number styles.</summary>
    public static sbyte? ToNSByte(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToSByte(provider, styles);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToSByte(this object target, out sbyte value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        sbyte.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>Parses a UInt16 using the current culture.</summary>
    public static ushort ToUShort(this object target) => ushort.Parse(target.ToString());

    /// <summary>Parses a UInt16 with the supplied culture and number styles.</summary>
    public static ushort ToUShort(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        ushort.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; otherwise parses a UInt16 using the current culture.</summary>
    public static ushort? ToNUShort(this object target) => target.IsNull() ? null : target.ToUShort();

    /// <summary>Returns null for null/DBNull; otherwise parses with the supplied culture and number styles.</summary>
    public static ushort? ToNUShort(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToUShort(provider, styles);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToUShort(this object target, out ushort value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        ushort.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>Parses a UInt32 using the current culture.</summary>
    public static uint ToUInt(this object target) => uint.Parse(target.ToString());

    /// <summary>Parses a UInt32 with the supplied culture and number styles.</summary>
    public static uint ToUInt(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        uint.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; otherwise parses a UInt32 using the current culture.</summary>
    public static uint? ToNUInt(this object target) => target.IsNull() ? null : target.ToUInt();

    /// <summary>Returns null for null/DBNull; otherwise parses with the supplied culture and number styles.</summary>
    public static uint? ToNUInt(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToUInt(provider, styles);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToUInt(this object target, out uint value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        uint.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>Parses a UInt64 using the current culture.</summary>
    public static ulong ToULong(this object target) => ulong.Parse(target.ToString());

    /// <summary>Parses a UInt64 with the supplied culture and number styles.</summary>
    public static ulong ToULong(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        ulong.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; otherwise parses a UInt64 using the current culture.</summary>
    public static ulong? ToNULong(this object target) => target.IsNull() ? null : target.ToULong();

    /// <summary>Returns null for null/DBNull; otherwise parses with the supplied culture and number styles.</summary>
    public static ulong? ToNULong(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToULong(provider, styles);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToULong(this object target, out ulong value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        ulong.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>Parses a native signed integer (IntPtr) using the current culture. The range follows the current process pointer size.</summary>
    public static nint ToIntPtr(this object target) => nint.Parse(target.ToString());

    /// <summary>Parses a native signed integer (IntPtr) with the supplied culture and number styles. The range follows the current process pointer size.</summary>
    public static nint ToIntPtr(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        nint.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; otherwise parses a native signed integer (IntPtr) using the current culture.</summary>
    public static nint? ToNIntPtr(this object target) => target.IsNull() ? null : target.ToIntPtr();

    /// <summary>Returns null for null/DBNull; otherwise parses with the supplied culture and number styles.</summary>
    public static nint? ToNIntPtr(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToIntPtr(provider, styles);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToIntPtr(this object target, out nint value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        nint.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>Parses a native unsigned integer (UIntPtr) using the current culture. The range follows the current process pointer size.</summary>
    public static nuint ToUIntPtr(this object target) => nuint.Parse(target.ToString());

    /// <summary>Parses a native unsigned integer (UIntPtr) with the supplied culture and number styles. The range follows the current process pointer size.</summary>
    public static nuint ToUIntPtr(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        nuint.Parse(GetParseText(target, provider), styles, provider);

    /// <summary>Returns null for null/DBNull; otherwise parses a native unsigned integer (UIntPtr) using the current culture.</summary>
    public static nuint? ToNUIntPtr(this object target) => target.IsNull() ? null : target.ToUIntPtr();

    /// <summary>Returns null for null/DBNull; otherwise parses with the supplied culture and number styles.</summary>
    public static nuint? ToNUIntPtr(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToUIntPtr(provider, styles);

    /// <inheritdoc cref="TryToShort(object, out short, IFormatProvider, NumberStyles)"/>
    public static bool TryToUIntPtr(this object target, out nuint value, IFormatProvider provider = null,
        NumberStyles styles = NumberStyles.Integer) =>
        nuint.TryParse(GetParseText(target, provider), styles, provider, out value);

    /// <summary>
    /// Parses exactly one UTF-16 code unit. Whitespace is a valid character; longer strings
    /// (including surrogate pairs) are rejected. Numeric inputs are parsed as text, not character codes.
    /// </summary>
    public static char ToChar(this object target) => char.Parse(target.ToString());

    /// <summary>Returns null for null/DBNull; otherwise parses exactly one UTF-16 code unit.</summary>
    public static char? ToNChar(this object target) => target.IsNull() ? null : target.ToChar();

    /// <summary>Returns false for null/DBNull or text that does not contain exactly one UTF-16 code unit.</summary>
    public static bool TryToChar(this object target, out char value) => char.TryParse(target?.ToString(), out value);

    /// <summary>Returns null for null/DBNull; otherwise parses an Int16 using the current culture.</summary>
    public static short? ToNShort(this object target) => target.IsNull() ? null : target.ToShort();

    /// <summary>Returns null for null/DBNull; otherwise parses an Int16 with the supplied culture and styles.</summary>
    public static short? ToNShort(this object target, IFormatProvider provider, NumberStyles styles = NumberStyles.Integer) =>
        target.IsNull() ? null : target.ToShort(provider, styles);

    /// <summary>Returns null for null/DBNull; otherwise parses a Single using the current culture.</summary>
    public static float? ToNFloat(this object target) => target.IsNull() ? null : target.ToFloat();

    /// <summary>Returns null for null/DBNull; otherwise parses a Single with the supplied culture and styles.</summary>
    public static float? ToNFloat(this object target, IFormatProvider provider,
        NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands) =>
        target.IsNull() ? null : target.ToFloat(provider, styles);

    // Format boxed numbers with the parsing culture as well. Using ordinary ToString here can
    // change 1.5m into "1,5" under a different current culture, which another parser may read as 15.
    private static string GetParseText(object target, IFormatProvider provider) =>
        target is IFormattable formattable ? formattable.ToString(null, provider) : target?.ToString();
}
