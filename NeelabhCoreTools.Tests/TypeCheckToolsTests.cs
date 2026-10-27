using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeelabhCoreTools.Tests;

public class TypeCheckToolsTests
{
    [Fact]
    public void PublishedSignaturesDefaultsAndNamedArgumentsRemainAvailable()
    {
        Assert.Null(typeof(TypeCheckTools).Namespace);
        foreach (var name in new[] { "IsInt", "IsLong", "IsFloat", "IsDecimal", "IsDouble", "IsBool", "IsDate" })
        {
            var method = typeof(TypeCheckTools).GetMethod(name, [typeof(string)])!;
            Assert.True(method.IsDefined(typeof(ExtensionAttribute)));
            Assert.Equal("value", method.GetParameters()[0].Name);
            Assert.Equal(typeof(bool), method.ReturnType);
        }
        foreach (var name in new[] { "IsNull", "IsNotNull", "IsEmpty", "IsNotEmpty" })
        {
            Assert.NotNull(typeof(TypeCheckTools).GetMethod(name, [typeof(object)]));
            Assert.NotNull(typeof(TypeCheckTools).GetMethod(name, [typeof(object), typeof(string)]));
            Assert.NotNull(typeof(TypeCheckTools).GetMethod(name, [typeof(object), typeof(string), typeof(string)]));
        }
        foreach (var name in new[] { "IsEqual", "IsNotEqual" })
        {
            var method = typeof(TypeCheckTools).GetMethod(name,
                [typeof(object), typeof(string), typeof(string), typeof(string)])!;
            Assert.Equal("", method.GetParameters()[3].DefaultValue);
            Assert.Equal(new[] { "target", "compareWith", "trueValue", "falseValue" },
                method.GetParameters().Select(parameter => parameter.Name));
        }
        Assert.Equal("match", "value".IsEqual(compareWith: "value", trueValue: "match"));
        Assert.Equal("", "value".IsNotEqual(compareWith: "value", trueValue: "different"));
    }

    [Fact]
    public void HasValueCorrectsMissingValuesWithoutChangingLegacyIsNotNull()
    {
        foreach (object? missing in new object?[] { null, DBNull.Value })
        {
            Assert.True(missing!.IsNull());
            Assert.True(missing!.IsNotNull()); // Published behavior, deliberately retained.
            Assert.Equal("present", missing!.IsNotNull("present"));
            Assert.Equal("present", missing!.IsNotNull("present", "missing"));
            Assert.False(missing!.HasValue());
            Assert.Equal("missing", missing!.HasValue("present", "missing"));
            Assert.Equal("", missing!.HasValue("present"));
            Assert.Equal("missing", missing!.IsNull("missing"));
        }
        Assert.True("".HasValue());
        Assert.True(0.HasValue());
        Assert.True(false.HasValue());
        Assert.False("".IsNull());
    }

    [Fact]
    public void LegacyTextChecksKeepNullExceptionsAndOriginalToStringBehavior()
    {
        object missing = null!;
        Assert.Throws<NullReferenceException>(() => missing.IsEmpty());
        Assert.Throws<NullReferenceException>(() => missing.IsNotEmpty());
        Assert.Throws<NullReferenceException>(() => missing.IsEqual("text"));
        Assert.Throws<NullReferenceException>(() => missing.IsNotEqual("text"));
        Assert.True(DBNull.Value.IsEmpty());
        Assert.True(DBNull.Value.IsNullOrEmpty());
        Assert.False(((object)new NullText()).IsNullOrEmpty());
        Assert.True(((object)new NullText()).IsNotNullOrEmpty());

        var changing = new ChangingText();
        Assert.Equal("second", changing.IsEmpty("empty"));
        Assert.Equal(2, changing.Calls);
    }

    [Fact]
    public void WhitespaceChecksAreNullSafeForStringsObjectsAndDatabaseNulls()
    {
        foreach (string? text in new[] { null, "", " \t\r\n", "\u2003" })
        {
            Assert.True(text!.IsNullOrWhiteSpace());
            Assert.False(text!.IsNotNullOrWhiteSpace());
            Assert.True(((object)text!).IsNullOrWhiteSpace());
            Assert.False(((object)text!).IsNotNullOrWhiteSpace());
        }
        Assert.True(DBNull.Value.IsNullOrWhiteSpace());
        Assert.True(new NullText().IsNullOrWhiteSpace());
        Assert.True(" x ".IsNotNullOrWhiteSpace());
        Assert.True(0.IsNotNullOrWhiteSpace());
        Assert.False(" ".IsNullOrEmpty()); // Whitespace did not become empty in the old API.
    }

    [Fact]
    public void TextComparisonHasExplicitNullAndCaseRules()
    {
        Assert.True(((object)null!).TextEquals(null!));
        Assert.True(DBNull.Value.TextEquals(null!));
        Assert.False(DBNull.Value.TextEquals(""));
        Assert.False("".TextEquals(null!));
        Assert.False("Name".TextEquals("name"));
        Assert.True("Name".TextEquals("name", StringComparison.OrdinalIgnoreCase));
        Assert.True("Name".TextNotEquals("name"));
        Assert.False("Name".TextNotEquals("name", StringComparison.OrdinalIgnoreCase));
        var failure = new InvalidOperationException("Custom text failed");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => new BrokenText(failure).TextEquals("x")));
    }

    [Fact]
    public void LegacyEmailChecksRetainTheirPatternAndNullException()
    {
        Assert.True("person@example.com".IsValidEmail());
        Assert.False("person+tag@example.com".IsValidEmail());
        Assert.False("person@example.technology".IsValidEmail());
        Assert.True("person@example.com\n".IsValidEmail());
        Assert.Throws<ArgumentNullException>(() => ((string)null!).IsValidEmail());
    }

    [Theory]
    [InlineData("person@example.com")]
    [InlineData("person+tag@example.com")]
    [InlineData("first.last@example.technology")]
    [InlineData("person@example.co.uk")]
    [InlineData("person@localhost")]
    public void MailboxValidationSupportsModernSuffixesAndPlusAddressing(string email)
    {
        Assert.True(email.IsValidEmailAddress());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-an-address")]
    [InlineData("@example.com")]
    [InlineData("person@@example.com")]
    [InlineData("Person <person@example.com>")]
    [InlineData("person@example.com,other@example.com")]
    [InlineData(" person@example.com")]
    [InlineData("person@example.com ")]
    [InlineData("person@example.com\n")]
    [InlineData("person@example.com\r\nBcc:other@example.com")]
    public void MailboxValidationRejectsMissingInputAndAddressWrappers(string? email)
    {
        Assert.False(email!.IsValidEmailAddress());
    }

    [Fact]
    public void StrictPhoneCheckRejectsSignsAndWhitespaceAcceptedByLegacyParsing()
    {
        using var culture = new CultureScope("en-US");
        foreach (var number in new[] { "+123456789", " 123456789", "123456789 " })
        {
            Assert.True(number.IsValid10DigitsMobile());
            Assert.False(number.IsValid10DigitsMobileStrict());
        }
        Assert.True("9876543210".IsValid10DigitsMobileStrict());
        foreach (string? number in new[] { null, "", "0123456789", "123456789", "12345678901", "12345a7890", "١٢٣٤٥٦٧٨٩٠" })
            Assert.False(number!.IsValid10DigitsMobileStrict());
    }

    [Fact]
    public void NumericChecksRetainCurrentCultureAndOfferExplicitCulturesAndStyles()
    {
        using var culture = new CultureScope("de-DE");
        var invariant = CultureInfo.InvariantCulture;
        Assert.True("1.234,5".IsDecimal());
        Assert.False("1,234.5".IsDecimal());
        Assert.True("1,234.5".IsDecimal(invariant));
        Assert.True("1,234.5".IsFloat(invariant));
        Assert.True("1,234.5".IsDouble(invariant));
        Assert.False("FF".IsInt());
        Assert.True("FF".IsInt(invariant, NumberStyles.HexNumber));
        Assert.True("1,234".IsLong(invariant, NumberStyles.AllowThousands));
        Assert.False("2147483648".IsInt());
        Assert.False("9223372036854775808".IsLong());
        Assert.False(((string)null!).IsDecimal(invariant));
        Assert.Throws<ArgumentException>(() => "1".IsInt(invariant, (NumberStyles)int.MaxValue));
    }

    [Fact]
    public void RemainingPrimitiveChecksEnforceRanges()
    {
        Assert.True("255".IsByte());
        Assert.False("256".IsByte());
        Assert.False("-1".IsByte());
        Assert.True("-128".IsSByte());
        Assert.False("128".IsSByte());
        Assert.True("32767".IsShort());
        Assert.False("32768".IsShort());
        Assert.True("65535".IsUShort());
        Assert.False("65536".IsUShort());
        Assert.True("4294967295".IsUInt());
        Assert.False("4294967296".IsUInt());
        Assert.True("18446744073709551615".IsULong());
        Assert.False("18446744073709551616".IsULong());
        Assert.True(nint.MaxValue.ToString().IsIntPtr());
        Assert.False((IntPtr.Size == 8 ? "9223372036854775808" : "2147483648").IsIntPtr());
        Assert.True(nuint.MaxValue.ToString().IsUIntPtr());
        Assert.False("-1".IsUIntPtr());
        Assert.True(" ".IsChar());
        Assert.True("λ".IsChar());
        Assert.False("🙂".IsChar());
        Assert.False(((string)null!).IsChar());
    }

    [Fact]
    public void PrimitiveStyleOverloadsMatchTheirConversionCounterparts()
    {
        var invariant = CultureInfo.InvariantCulture;
        Assert.True("FF".IsByte(invariant, NumberStyles.HexNumber));
        Assert.True("FF".IsSByte(invariant, NumberStyles.HexNumber));
        Assert.True("FFFF".IsShort(invariant, NumberStyles.HexNumber));
        Assert.True("FFFF".IsUShort(invariant, NumberStyles.HexNumber));
        Assert.True("FFFFFFFF".IsUInt(invariant, NumberStyles.HexNumber));
        Assert.True("FFFFFFFFFFFFFFFF".IsULong(invariant, NumberStyles.HexNumber));
        Assert.True("FF".IsIntPtr(invariant, NumberStyles.HexNumber));
        Assert.True("FF".IsUIntPtr(invariant, NumberStyles.HexNumber));
        Assert.False("-1".IsUInt(invariant));
    }

    [Fact]
    public void FiniteChecksExcludeNonFiniteValuesWithoutChangingLegacyFloatChecks()
    {
        using var culture = new CultureScope("");
        foreach (var text in new[] { "NaN", "Infinity", "-Infinity", "1e999" })
        {
            Assert.True(text.IsFloat());
            Assert.True(text.IsDouble());
            Assert.False(text.IsFiniteFloat());
            Assert.False(text.IsFiniteDouble());
        }
        Assert.True("1,5".IsFiniteFloat(CultureInfo.GetCultureInfo("fr-FR")));
        Assert.True("1.5e20".IsFiniteDouble(CultureInfo.InvariantCulture));
        Assert.False(((string)null!).IsFiniteDouble());
    }

    [Fact]
    public void ExactDatesAndEnumsHaveExplicitValidationPolicies()
    {
        using var culture = new CultureScope("en-US");
        var french = CultureInfo.GetCultureInfo("fr-FR");
        Assert.False("31/12/2024".IsDate());
        Assert.True("31/12/2024".IsDate(french));
        Assert.True("31/12/2024".IsDateExact());
        Assert.True("02 janvier 2024".IsDateExact("dd MMMM yyyy", french));
        Assert.False("31/02/2024".IsDateExact());
        Assert.False(((string)null!).IsDateExact());
        Assert.Throws<ArgumentException>(() => "2024-01-02".IsDate(french,
            DateTimeStyles.AssumeLocal | DateTimeStyles.AssumeUniversal));
        Assert.True("read".IsEnum<Access>());
        Assert.False("read".IsEnum<Access>(ignoreCase: false));
        Assert.True("Read".IsEnum<Access>(requireDefined: true));
        Assert.True("99".IsEnum<Access>());
        Assert.False("99".IsEnum<Access>(requireDefined: true));
        Assert.True("Read, Write".IsEnum<Access>());
        Assert.False("Read, Write".IsEnum<Access>(requireDefined: true));
        Assert.False(((string)null!).IsEnum<Access>());
    }

    [Flags]
    private enum Access { Read = 1, Write = 2 }

    private sealed class NullText { public override string ToString() => null!; }
    private sealed class BrokenText(Exception failure) { public override string ToString() => throw failure; }
    private sealed class ChangingText
    {
        public int Calls { get; private set; }
        public override string ToString() => ++Calls == 1 ? "first" : "second";
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo original = CultureInfo.CurrentCulture;
        public CultureScope(string name) => CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
        public void Dispose() => CultureInfo.CurrentCulture = original;
    }
}
