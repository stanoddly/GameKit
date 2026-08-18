using System.Text;

namespace Pixely.Tests;

public class Base40EncodingTests
{
    private const ulong MaximumUInt64Value = 16_777_215_999_999_999_999UL;

    [TestCase(0UL, "000000000000")]
    [TestCase(1UL, "000000000001")]
    [TestCase(36UL, "00000000000-")]
    [TestCase(37UL, "00000000000_")]
    [TestCase(38UL, "00000000000.")]
    [TestCase(39UL, "00000000000~")]
    [TestCase(40UL, "000000000010")]
    [TestCase(MaximumUInt64Value, "~~~~~~~~~~~~")]
    public void Encode_UInt64_ReturnsExpectedValue(ulong value, string expected)
    {
        string encodedValue = Base40Encoding.Encode(value);

        Assert.That(encodedValue, Is.EqualTo(expected));
    }

    [Test]
    public void Encode_UInt64OutsideCapacity_ThrowsArgumentOutOfRangeException()
    {
        const ulong firstUnsupportedValue = 16_777_216_000_000_000_000UL;

        Assert.That(
            () => Base40Encoding.Encode(firstUnsupportedValue),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Encode_UInt128_ReturnsFixedWidthValue()
    {
        string encodedValue = Base40Encoding.Encode((UInt128)40);

        Assert.That(encodedValue, Is.EqualTo("0000000000000000000000010"));
    }

    [TestCase("000000000000", 0UL)]
    [TestCase("000000000001", 1UL)]
    [TestCase("00000000000-", 36UL)]
    [TestCase("00000000000_", 37UL)]
    [TestCase("00000000000.", 38UL)]
    [TestCase("00000000000~", 39UL)]
    [TestCase("000000000010", 40UL)]
    [TestCase("~~~~~~~~~~~~", MaximumUInt64Value)]
    public void TryDecode_UInt64Characters_ReturnsExpectedValue(string encodedValue, ulong expected)
    {
        bool success = Base40Encoding.TryDecode(encodedValue, out ulong value);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(expected));
        });
    }

    [TestCase("")]
    [TestCase("00000000000")]
    [TestCase("0000000000000")]
    [TestCase("00000000000A")]
    [TestCase("00000000000/")]
    [TestCase("00000000000 ")]
    public void TryDecode_InvalidUInt64Characters_ReturnsFalse(string encodedValue)
    {
        bool success = Base40Encoding.TryDecode(encodedValue, out ulong value);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.False);
            Assert.That(value, Is.Zero);
        });
    }

    [Test]
    public void TryDecode_UInt64Utf8Value_ReturnsExpectedValue()
    {
        bool success = Base40Encoding.TryDecode("00000000000~"u8, out ulong value);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(39UL));
        });
    }

    [TestCase(2UL)]
    [TestCase(38UL)]
    [TestCase(41UL)]
    [TestCase(123_456_789UL)]
    [TestCase(ulong.MaxValue / 2)]
    [TestCase(MaximumUInt64Value - 1)]
    public void EncodeAndTryDecode_UInt64_RoundTrips(ulong value)
    {
        string encodedValue = Base40Encoding.Encode(value);

        bool success = Base40Encoding.TryDecode(encodedValue, out ulong decodedValue);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(decodedValue, Is.EqualTo(value));
        });
    }

    [Test]
    public void TryDecode_UInt128Utf8Value_ReturnsExpectedValue()
    {
        bool success = Base40Encoding.TryDecode("000000000000000000000000~"u8, out UInt128 value);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            // NUnit2021 false positive: analyzer does not recognize UInt128 equality
            Assert.That(value == (UInt128)39, Is.True);
        });
    }

    [Test]
    public void TryDecode_UInt128Overflow_ReturnsFalse()
    {
        bool characterSuccess = Base40Encoding.TryDecode("~~~~~~~~~~~~~~~~~~~~~~~~~", out UInt128 characterValue);
        bool byteSuccess = Base40Encoding.TryDecode("~~~~~~~~~~~~~~~~~~~~~~~~~"u8, out UInt128 byteValue);

        Assert.Multiple(() =>
        {
            Assert.That(characterSuccess, Is.False);
            // NUnit2021 false positive: analyzer does not recognize UInt128 equality
            Assert.That(characterValue == UInt128.Zero, Is.True);
            Assert.That(byteSuccess, Is.False);
            Assert.That(byteValue == UInt128.Zero, Is.True);
        });
    }

    [Test]
    public void TryDecode_InvalidUInt128Value_ReturnsFalse()
    {
        bool invalidLengthSuccess = Base40Encoding.TryDecode("000000000000000000000000", out UInt128 invalidLengthValue);
        bool invalidCharacterSuccess = Base40Encoding.TryDecode("000000000000000000000000A", out UInt128 invalidCharacterValue);

        Assert.Multiple(() =>
        {
            Assert.That(invalidLengthSuccess, Is.False);
            // NUnit2021 false positive: analyzer does not recognize UInt128 equality
            Assert.That(invalidLengthValue == UInt128.Zero, Is.True);
            Assert.That(invalidCharacterSuccess, Is.False);
            Assert.That(invalidCharacterValue == UInt128.Zero, Is.True);
        });
    }

    [Test]
    public void EncodeAndTryDecode_MaximumUInt128_RoundTripsCharactersAndBytes()
    {
        string encodedValue = Base40Encoding.Encode(UInt128.MaxValue);
        byte[] encodedBytes = Encoding.ASCII.GetBytes(encodedValue);

        bool characterSuccess = Base40Encoding.TryDecode(encodedValue, out UInt128 characterValue);
        bool byteSuccess = Base40Encoding.TryDecode(encodedBytes, out UInt128 byteValue);

        Assert.Multiple(() =>
        {
            Assert.That(encodedValue, Has.Length.EqualTo(25));
            Assert.That(characterSuccess, Is.True);
            Assert.That(characterValue, Is.EqualTo(UInt128.MaxValue));
            Assert.That(byteSuccess, Is.True);
            Assert.That(byteValue, Is.EqualTo(UInt128.MaxValue));
        });
    }
}
