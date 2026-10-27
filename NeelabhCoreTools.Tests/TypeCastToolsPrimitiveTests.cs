using System.Globalization;

namespace NeelabhCoreTools.Tests;

public class TypeCastToolsPrimitiveTests
{
    [Fact]
    public void ByteAndSByteConversionsEnforceTheirRanges()
    {
        CheckInteger(byte.MinValue, byte.MaxValue, "-1", "256",
            TypeCastTools.ToByte, TypeCastTools.ToNByte, TypeCastTools.TryToByte);
        CheckInteger(sbyte.MinValue, sbyte.MaxValue, "-129", "128",
            TypeCastTools.ToSByte, TypeCastTools.ToNSByte, TypeCastTools.TryToSByte);
    }

    [Fact]
    public void UnsignedIntegerConversionsRejectNegativeValuesAndOverflow()
    {
        CheckInteger(ushort.MinValue, ushort.MaxValue, "-1", "65536",
            TypeCastTools.ToUShort, TypeCastTools.ToNUShort, TypeCastTools.TryToUShort);
        CheckInteger(uint.MinValue, uint.MaxValue, "-1", "4294967296",
            TypeCastTools.ToUInt, TypeCastTools.ToNUInt, TypeCastTools.TryToUInt);
        CheckInteger(ulong.MinValue, ulong.MaxValue, "-1", "18446744073709551616",
            TypeCastTools.ToULong, TypeCastTools.ToNULong, TypeCastTools.TryToULong);
    }

    [Fact]
    public void NativeIntegersFollowTheCurrentProcessWidth()
    {
        CheckInteger(nint.MinValue, nint.MaxValue,
            IntPtr.Size == 8 ? "-9223372036854775809" : "-2147483649",
            IntPtr.Size == 8 ? "9223372036854775808" : "2147483648",
            TypeCastTools.ToIntPtr, TypeCastTools.ToNIntPtr, TypeCastTools.TryToIntPtr);
        CheckInteger(nuint.MinValue, nuint.MaxValue, "-1",
            UIntPtr.Size == 8 ? "18446744073709551616" : "4294967296",
            TypeCastTools.ToUIntPtr, TypeCastTools.ToNUIntPtr, TypeCastTools.TryToUIntPtr);

        string allBits = new('F', IntPtr.Size * 2);
        Assert.Equal((nint)(-1), allBits.ToIntPtr(CultureInfo.InvariantCulture, NumberStyles.HexNumber));
        Assert.Equal(nuint.MaxValue, allBits.ToUIntPtr(CultureInfo.InvariantCulture, NumberStyles.HexNumber));
        Assert.Equal(typeof(int?), typeof(TypeCastTools).GetMethod("ToNInt", [typeof(object)])!.ReturnType);
    }

    [Fact]
    public void IntegerProviderOverloadsSupportHexWithoutChangingDecimalDefaults()
    {
        var invariant = CultureInfo.InvariantCulture;
        Assert.Throws<FormatException>(() => "FF".ToByte());
        Assert.Equal(byte.MaxValue, "FF".ToByte(invariant, NumberStyles.HexNumber));
        Assert.Equal((sbyte)(-1), "FF".ToSByte(invariant, NumberStyles.HexNumber));
        Assert.Equal(ushort.MaxValue, "FFFF".ToUShort(invariant, NumberStyles.HexNumber));
        Assert.Equal(uint.MaxValue, "FFFFFFFF".ToUInt(invariant, NumberStyles.HexNumber));
        Assert.Equal(ulong.MaxValue, "FFFFFFFFFFFFFFFF".ToULong(invariant, NumberStyles.HexNumber));
        Assert.Equal(byte.MaxValue, "FF".ToNByte(invariant, NumberStyles.HexNumber));
        Assert.Equal((sbyte)(-1), "FF".ToNSByte(invariant, NumberStyles.HexNumber));
        Assert.Equal(ushort.MaxValue, "FFFF".ToNUShort(invariant, NumberStyles.HexNumber));
        Assert.Equal(uint.MaxValue, "FFFFFFFF".ToNUInt(invariant, NumberStyles.HexNumber));
        Assert.Equal(ulong.MaxValue, "FFFFFFFFFFFFFFFF".ToNULong(invariant, NumberStyles.HexNumber));
        Assert.Equal((nint)255, "FF".ToNIntPtr(invariant, NumberStyles.HexNumber));
        Assert.Equal((nuint)255, "FF".ToNUIntPtr(invariant, NumberStyles.HexNumber));
        Assert.Throws<ArgumentException>(() => "1".TryToByte(out _, styles: (NumberStyles)int.MaxValue));
    }

    [Fact]
    public void ExplicitCulturesAreUsedForBoxedIntegerFormattingAsWellAsParsing()
    {
        var provider = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        provider.NegativeSign = "minus";
        Assert.Equal((sbyte)(-12), "minus12".ToSByte(provider));
        Assert.Equal((sbyte)(-12), ((object)(sbyte)(-12)).ToSByte(provider));
        Assert.True(((object)(nint)(-12)).TryToIntPtr(out var native, provider));
        Assert.Equal((nint)(-12), native);
        Assert.False("minus1".TryToUInt(out _, provider));
        Assert.Equal(1234U, "1,234".ToUInt(CultureInfo.InvariantCulture, NumberStyles.AllowThousands));
        Assert.True("1,234".TryToULong(out var large, CultureInfo.InvariantCulture, NumberStyles.AllowThousands));
        Assert.Equal(1234UL, large);
        Assert.True("1.0".TryToByte(out var whole, CultureInfo.InvariantCulture, NumberStyles.AllowDecimalPoint));
        Assert.Equal((byte)1, whole);
        Assert.False("1.5".TryToByte(out _, CultureInfo.InvariantCulture, NumberStyles.AllowDecimalPoint));
    }

    [Theory]
    [InlineData("A", 'A')]
    [InlineData(" ", ' ')]
    [InlineData("\n", '\n')]
    [InlineData("\0", '\0')]
    [InlineData("λ", 'λ')]
    public void CharacterConversionsAcceptExactlyOneUtf16CodeUnit(string text, char expected)
    {
        Assert.Equal(expected, text.ToChar());
        Assert.Equal(expected, text.ToNChar());
        Assert.True(text.TryToChar(out var value));
        Assert.Equal(expected, value);
        Assert.Equal(expected, ((object)expected).ToChar());
    }

    [Fact]
    public void CharacterConversionsDoNotTrimTextOrInterpretNumericCharacterCodes()
    {
        foreach (var text in new[] { "", "AB", " A", "65", "🙂" })
        {
            Assert.Throws<FormatException>(() => text.ToChar());
            Assert.Throws<FormatException>(() => text.ToNChar());
            Assert.False(text.TryToChar(out var character));
            Assert.Equal('\0', character);
        }
        Assert.Equal('7', 7.ToChar());
        Assert.Throws<FormatException>(() => 65.ToChar());
        Assert.Throws<NullReferenceException>(() => ((object)null!).ToChar());
        Assert.Throws<FormatException>(() => DBNull.Value.ToChar());
        Assert.Null(((object)null!).ToNChar());
        Assert.Null(DBNull.Value.ToNChar());
        Assert.False(((object)null!).TryToChar(out _));
        Assert.False(DBNull.Value.TryToChar(out _));
    }

    [Fact]
    public void NullableShortAndFloatHelpersMatchTheExistingNullablePolicy()
    {
        var french = CultureInfo.GetCultureInfo("fr-FR");
        foreach (var target in new object?[] { null, DBNull.Value })
        {
            Assert.Null(target!.ToNShort());
            Assert.Null(target!.ToNShort(french));
            Assert.Null(target!.ToNFloat());
            Assert.Null(target!.ToNFloat(french));
        }
        Assert.Equal(short.MinValue, "-32768".ToNShort(CultureInfo.InvariantCulture));
        Assert.Equal(short.MaxValue, "32767".ToNShort());
        Assert.Equal((short)255, "FF".ToNShort(CultureInfo.InvariantCulture, NumberStyles.HexNumber));
        Assert.Equal(1.5f, "1,5".ToNFloat(french));
        Assert.Equal(1.5f, ((object)1.5f).ToNFloat(french));
        Assert.True(float.IsNaN("NaN".ToNFloat(CultureInfo.InvariantCulture)!.Value));
        Assert.Throws<FormatException>(() => "".ToNShort());
        Assert.Throws<OverflowException>(() => "32768".ToNShort());
        Assert.Throws<FormatException>(() => "".ToNFloat());
    }

    [Fact]
    public void NullableIntegerProviderOverloadsKeepMissingValuesNull()
    {
        var provider = CultureInfo.InvariantCulture;
        foreach (var target in new object?[] { null, DBNull.Value })
        {
            Assert.Null(target!.ToNByte(provider));
            Assert.Null(target!.ToNSByte(provider));
            Assert.Null(target!.ToNUShort(provider));
            Assert.Null(target!.ToNUInt(provider));
            Assert.Null(target!.ToNULong(provider));
            Assert.Null(target!.ToNIntPtr(provider));
            Assert.Null(target!.ToNUIntPtr(provider));
        }
    }

    private delegate bool TryInteger<T>(object target, out T value, IFormatProvider provider, NumberStyles styles);

    private static void CheckInteger<T>(T minimum, T maximum, string tooSmall, string tooLarge,
        Func<object, T> parse, Func<object, T?> parseNullable, TryInteger<T> tryParse) where T : struct, IFormattable
    {
        foreach (var boundary in new[] { minimum, maximum })
        {
            var text = boundary.ToString(null, CultureInfo.InvariantCulture);
            Assert.Equal(boundary, parse(text));
            Assert.Equal(boundary, parseNullable(text));
            Assert.True(tryParse(text, out var actual, CultureInfo.InvariantCulture, NumberStyles.Integer));
            Assert.Equal(boundary, actual);
        }
        foreach (var text in new[] { tooSmall, tooLarge })
        {
            Assert.Throws<OverflowException>(() => parse(text));
            Assert.Throws<OverflowException>(() => parseNullable(text));
            Assert.False(tryParse(text, out var actual, CultureInfo.InvariantCulture, NumberStyles.Integer));
            Assert.Equal(default, actual);
        }
        foreach (object? input in new object?[] { null, DBNull.Value, "", "not an integer" })
        {
            Assert.False(tryParse(input!, out var actual, CultureInfo.InvariantCulture, NumberStyles.Integer));
            Assert.Equal(default, actual);
        }
        Assert.Throws<NullReferenceException>(() => parse(null!));
        Assert.Throws<FormatException>(() => parse(DBNull.Value));
        Assert.Throws<FormatException>(() => parse(""));
        Assert.Throws<FormatException>(() => parseNullable(""));
        Assert.Null(parseNullable(null!));
        Assert.Null(parseNullable(DBNull.Value));
    }
}
