using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeelabhCoreTools.Tests;

public class TypeCastToolsTests
{
    [Fact]
    public void PublishedDefaultsParameterNamesAndGenericConstraintsAreUnchanged()
    {
        Assert.Null(typeof(TypeCastTools).Namespace);
        var exact = LegacyMethod("ToDateExact", 2);
        Assert.Equal("format", exact.GetParameters()[1].Name);
        Assert.Equal("dd/MM/yyyy", exact.GetParameters()[1].DefaultValue);
        Assert.Equal("dd/MM/yyyy", LegacyMethod("ToNDateExact", 2).GetParameters()[1].DefaultValue);
        var format = LegacyMethod("FormatDate", 2);
        Assert.Equal("foramt", format.GetParameters()[1].Name);
        Assert.Equal("dd-MMM-yyyy", format.GetParameters()[1].DefaultValue);
        var enumMethod = LegacyMethod("ToEnum", 1);
        Assert.Empty(enumMethod.GetGenericArguments()[0].GetGenericParameterConstraints());
        Assert.Equal(GenericParameterAttributes.None, enumMethod.GetGenericArguments()[0].GenericParameterAttributes);
        foreach (var name in new[] { "ToBool", "ToShort", "ToInt", "ToLong", "ToFloat", "ToDouble", "ToDecimal",
            "ToEnum", "ToNBool", "ToNInt", "ToNLong", "ToNDecimal", "ToNDouble", "ToDate", "ToNDate" })
        {
            var method = LegacyMethod(name, 1);
            Assert.Equal("target", method.GetParameters()[0].Name);
            Assert.Equal(typeof(object), method.GetParameters()[0].ParameterType);
            Assert.True(method.IsDefined(typeof(ExtensionAttribute)));
        }
        using var culture = new CultureScope("en-GB");
        Assert.Equal("02-Jan-2024", "02/01/2024".FormatDate(foramt: "dd-MMM-yyyy"));
        Assert.Equal(new DateTime(2024, 1, 2), "02/01/2024".ToDateExact(format: "dd/MM/yyyy"));
    }

    [Fact]
    public void LegacyNumericParsingKeepsCurrentCultureAndDoesNotRoundIntegers()
    {
        using var culture = new CultureScope("de-DE");
        Assert.Equal(1234.5m, "1.234,5".ToDecimal());
        Assert.Equal(1234.5, "1.234,5".ToDouble());
        Assert.Equal(12.5f, "12,5".ToFloat());
        Assert.Equal(12.5m, ((object)12.5m).ToNDecimal());
        Assert.Equal(12, " 12 ".ToInt());
        Assert.Throws<FormatException>(() => "1,5".ToInt());
        Assert.Throws<FormatException>(() => ((object)1.5m).ToInt());
        Assert.Throws<OverflowException>(() => "2147483648".ToInt());
        Assert.Throws<OverflowException>(() => "32768".ToShort());
        Assert.Throws<OverflowException>(() => "9223372036854775808".ToLong());
    }

    [Fact]
    public void LegacyThrowingMethodsKeepNullAndDatabaseNullFailures()
    {
        foreach (var convert in ThrowingConversions())
        {
            Assert.Throws<NullReferenceException>(() => convert(null!));
            Assert.Throws<FormatException>(() => convert(DBNull.Value));
            Assert.Throws<FormatException>(() => convert(""));
        }
    }

    [Fact]
    public void NullableConversionsOnlyTreatMissingValuesAsNull()
    {
        var conversions = new Func<object, object?>[]
        {
            value => value.ToNBool(), value => value.ToNInt(), value => value.ToNLong(),
            value => value.ToNDecimal(), value => value.ToNDouble(),
            value => value.ToNDate(), value => value.ToNDateExact()
        };
        foreach (var convert in conversions)
        {
            Assert.Null(convert(null!));
            Assert.Null(convert(DBNull.Value));
            Assert.Throws<FormatException>(() => convert(""));
            Assert.Throws<FormatException>(() => convert("not a value"));
        }
    }

    [Fact]
    public void LegacyDatesKeepCurrentCultureAndInvariantExactDateDefaults()
    {
        using (new CultureScope("en-US"))
        {
            Assert.Equal(new DateTime(2024, 2, 1), "02/01/2024".ToDate());
            Assert.Equal(new DateTime(2024, 1, 2), "02/01/2024".ToDateExact());
        }
        using (new CultureScope("en-GB"))
        {
            Assert.Equal(new DateTime(2024, 1, 2), "02/01/2024".ToDate());
            Assert.Equal(new DateTime(2024, 1, 2), "02/01/2024".ToNDateExact());
        }
        using (new CultureScope("fr-FR"))
        {
            Assert.Equal(new DateTime(2024, 1, 2), "02 January 2024".ToDateExact("dd MMMM yyyy"));
            Assert.Throws<FormatException>(() => "02 janvier 2024".ToDateExact("dd MMMM yyyy"));
        }
    }

    [Fact]
    public void LegacyDateTimeRoundTripIsPreservedWhileExplicitFormattingKeepsPrecision()
    {
        using var culture = new CultureScope("");
        object source = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc).AddTicks(1234567);
        Assert.Equal("2024-01-02T03:04:05.0000000", source.FormatDate(foramt: "O"));
        Assert.Equal(DateTimeKind.Unspecified, source.ToDate().Kind);
        Assert.Equal(0, source.ToDate().Ticks % TimeSpan.TicksPerSecond);
        Assert.Equal("2024-01-02T03:04:05.1234567Z", source.FormatDate("O", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void EnumNamesUndefinedNumbersAndFlagsRetainTheirExistingMeaning()
    {
        Assert.Equal(Access.Read, "read".ToEnum<Access>());
        Assert.Equal(Access.Read | Access.Write, "Read, Write".ToEnum<Access>());
        Assert.Equal((Access)99, "99".ToEnum<Access>());
        Assert.Throws<ArgumentException>(() => "1".ToEnum<int>());
        Assert.Throws<ArgumentException>(() => "unknown".ToEnum<Access>());
        Assert.True("read".TryToEnum<Access>(out var access));
        Assert.Equal(Access.Read, access);
        Assert.False("read".TryToEnum<Access>(out access, ignoreCase: false));
        Assert.Equal(default, access);
        Assert.True("99".TryToEnum<Access>(out access));
        Assert.Equal((Access)99, access);
        Assert.True("Read, Write".TryToEnum<Access>(out access));
        Assert.Equal(Access.Read | Access.Write, access);
        Assert.False(((object)null!).TryToEnum<Access>(out _));
        Assert.False(DBNull.Value.TryToEnum<Access>(out _));
        Assert.False("999999999999999999999".TryToEnum<Access>(out _));
    }

    [Fact]
    public void ExplicitCultureAppliesToBothBoxedNumberFormattingAndParsing()
    {
        using var culture = new CultureScope("de-DE");
        var invariant = CultureInfo.InvariantCulture;
        Assert.Equal(1.5m, ((object)1.5m).ToDecimal(invariant));
        Assert.True(((object)1.5m).TryToDecimal(out var number, invariant));
        Assert.Equal(1.5m, number);
        Assert.Equal(1234.5m, "1,234.5".ToDecimal(invariant));
        Assert.Equal(1234.5m, "1,234.5".ToNDecimal(invariant));
        Assert.True("1,234.5".TryToDouble(out var floating, invariant));
        Assert.Equal(1234.5, floating);
        Assert.True("12,5".TryToDecimal(out number));
        Assert.Equal(12.5m, number);
        Assert.Null(DBNull.Value.ToNDecimal(invariant));
        Assert.Throws<FormatException>(() => "".ToNDecimal(invariant));
    }

    [Fact]
    public void ExplicitNumberStylesAllowOptInFormatsWithoutChangingDefaults()
    {
        var invariant = CultureInfo.InvariantCulture;
        Assert.Throws<FormatException>(() => "FF".ToInt());
        Assert.Equal(255, "FF".ToInt(invariant, NumberStyles.HexNumber));
        Assert.True("FF".TryToInt(out var hex, styles: NumberStyles.HexNumber));
        Assert.Equal(255, hex);
        Assert.Equal((short)255, "FF".ToShort(invariant, NumberStyles.HexNumber));
        Assert.Equal(255L, "FF".ToLong(invariant, NumberStyles.HexNumber));
        Assert.Equal(255, "FF".ToNInt(invariant, NumberStyles.HexNumber));
        Assert.Equal(255L, "FF".ToNLong(invariant, NumberStyles.HexNumber));
        Assert.True("1e2".TryToInt(out var exponent, invariant, NumberStyles.Float));
        Assert.Equal(100, exponent);
        Assert.True("1,234".TryToLong(out var grouped, invariant, NumberStyles.AllowThousands));
        Assert.Equal(1234L, grouped);
    }

    [Fact]
    public void TryMethodsRejectInvalidInputAndResetOutputValues()
    {
        foreach (object? target in new object?[] { null, DBNull.Value, "", "not a value" })
        {
            Assert.False(target!.TryToBool(out var boolean));
            Assert.False(boolean);
            Assert.False(target!.TryToShort(out var small));
            Assert.Equal(0, small);
            Assert.False(target!.TryToInt(out var integer));
            Assert.Equal(0, integer);
            Assert.False(target!.TryToLong(out var large));
            Assert.Equal(0, large);
            Assert.False(target!.TryToFloat(out var single));
            Assert.Equal(0, single);
            Assert.False(target!.TryToDouble(out var floating));
            Assert.Equal(0, floating);
            Assert.False(target!.TryToDecimal(out var number));
            Assert.Equal(0, number);
            Assert.False(target!.TryToDate(out var date));
            Assert.Equal(default, date);
            Assert.False(target!.TryToDateExact(out date));
            Assert.Equal(default, date);
        }
        Assert.False("32768".TryToShort(out _));
        Assert.False("2147483648".TryToInt(out _));
        Assert.False("9223372036854775808".TryToLong(out _));
        Assert.False("79228162514264337593543950336".TryToDecimal(out _));
        Assert.False("1.5".TryToInt(out _, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void BooleanParsingDoesNotTreatNumbersAsTrueOrFalse()
    {
        Assert.True(" TrUe ".ToBool());
        Assert.True(" TrUe ".TryToBool(out var value));
        Assert.True(value);
        Assert.True(((object)false).TryToBool(out value));
        Assert.False(value);
        Assert.False(1.TryToBool(out _));
        Assert.False("0".TryToBool(out _));
        Assert.Throws<FormatException>(() => 1.ToBool());
    }

    [Fact]
    public void FloatingPointSpecialValuesKeepRuntimeParsingSemantics()
    {
        var invariant = CultureInfo.InvariantCulture;
        Assert.True("NaN".TryToDouble(out var nan, invariant));
        Assert.True(double.IsNaN(nan));
        Assert.True("1e999".TryToFloat(out var infinity, invariant));
        Assert.True(float.IsPositiveInfinity(infinity));
        Assert.True("1e999".TryToDouble(out var big, invariant));
        Assert.True(double.IsPositiveInfinity(big));
        Assert.Equal(12.5f, "12.5".ToFloat(invariant));
        Assert.Equal(12.5, "12.5".ToDouble(invariant));
        Assert.Equal(12.5, "12.5".ToNDouble(invariant));
    }

    [Fact]
    public void DateOverloadsSupportExplicitCulturesAndRoundTripStyles()
    {
        using var culture = new CultureScope("en-US");
        var french = CultureInfo.GetCultureInfo("fr-FR");
        var expected = new DateTime(2024, 1, 2);
        Assert.Equal(expected, "02/01/2024".ToDate(french));
        Assert.Equal(expected, "02/01/2024".ToNDate(french));
        Assert.Equal(expected, "02 janvier 2024".ToDateExact("dd MMMM yyyy", french));
        Assert.Equal(expected, "02 janvier 2024".ToNDateExact("dd MMMM yyyy", french));
        Assert.Equal("02 janvier 2024", "02/01/2024".FormatDate("dd MMMM yyyy", french));
        Assert.True("02 janvier 2024".TryToDateExact(out var date, "dd MMMM yyyy", french));
        Assert.Equal(expected, date);
        Assert.True("02/01/2024".TryToDateExact(out date));
        Assert.Equal(expected, date);
        Assert.True("02/01/2024".TryToDate(out date, french));
        Assert.Equal(expected, date);
        const string roundTrip = "2024-01-02T03:04:05.1234567Z";
        Assert.True(roundTrip.TryToDateExact(out date, "O", styles: DateTimeStyles.RoundtripKind));
        Assert.Equal(DateTimeKind.Utc, date.Kind);
        Assert.Equal(1234567, date.Ticks % TimeSpan.TicksPerSecond);
        Assert.Null(DBNull.Value.ToNDate(french));
        Assert.Null(DBNull.Value.ToNDateExact("dd MMMM yyyy", french));
        Assert.False("31/02/2024".TryToDateExact(out _));
    }

    [Fact]
    public void TryMethodsDoNotHideInvalidConfigurationOrCustomFormattingFailures()
    {
        Assert.Throws<ArgumentException>(() => "1".TryToInt(out _, styles: (NumberStyles)int.MaxValue));
        Assert.Throws<ArgumentException>(() => "2024-01-02".TryToDate(out _,
            styles: DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal));
        var failure = new InvalidOperationException("Formatting failed");
        var input = new BrokenText(failure);
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => input.TryToInt(out _)));
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => input.TryToBool(out _)));
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => input.TryToDate(out _)));
    }

    private static MethodInfo LegacyMethod(string name, int parameterCount) => typeof(TypeCastTools)
        .GetMethods().Single(method => method.Name == name && method.GetParameters().Length == parameterCount);

    private static IEnumerable<Func<object, object>> ThrowingConversions() =>
    [
        value => value.ToBool(), value => value.ToShort(), value => value.ToInt(), value => value.ToLong(),
        value => value.ToFloat(), value => value.ToDouble(), value => value.ToDecimal(),
        value => value.ToDate(), value => value.ToDateExact(), value => value.FormatDate()
    ];

    [Flags]
    private enum Access { Read = 1, Write = 2 }

    private sealed class BrokenText(Exception failure)
    {
        public override string ToString() => throw failure;
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo original = CultureInfo.CurrentCulture;
        public CultureScope(string name) => CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
        public void Dispose() => CultureInfo.CurrentCulture = original;
    }
}
