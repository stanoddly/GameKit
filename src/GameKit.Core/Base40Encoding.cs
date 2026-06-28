namespace GameKit;

/// <summary>
/// Encodes non-negative integers as fixed-width, URL-safe base-40 strings.
/// </summary>
public static class Base40Encoding
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz-_.~";
    private const ulong ExclusiveUpperBound = 16_777_216_000_000_000_000UL;
    private const int Radix = 40;
    private const int UInt64Width = 12;
    private const int UInt128Width = 25;

    /// <summary>
    /// Encodes a supported value as exactly 12 characters.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is greater than or equal to 40^12.
    /// </exception>
    public static string Encode(ulong value)
    {
        if (value >= ExclusiveUpperBound)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Value must be less than {ExclusiveUpperBound}.");
        }

        return string.Create(UInt64Width, value, static (characters, remainingValue) =>
        {
            for (int index = UInt64Width - 1; index >= 0; index--)
            {
                characters[index] = Alphabet[(int)(remainingValue % Radix)];
                remainingValue /= Radix;
            }
        });
    }

    /// <summary>
    /// Encodes a value as exactly 25 characters.
    /// </summary>
    public static string Encode(UInt128 value)
    {
        return string.Create(UInt128Width, value, static (characters, remainingValue) =>
        {
            for (int index = UInt128Width - 1; index >= 0; index--)
            {
                characters[index] = Alphabet[(int)(remainingValue % Radix)];
                remainingValue /= Radix;
            }
        });
    }

    /// <summary>
    /// Attempts to decode an exactly 12-character base-40 value.
    /// </summary>
    /// <returns><see langword="true"/> when decoding succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryDecode(ReadOnlySpan<char> encodedValue, out ulong value)
    {
        value = default;
        if (encodedValue.Length != UInt64Width)
        {
            return false;
        }

        ulong decodedValue = 0;
        for (int index = 0; index < encodedValue.Length; index++)
        {
            int digit = DecodeDigit(encodedValue[index]);
            if (digit < 0)
            {
                return false;
            }

            decodedValue = decodedValue * Radix + (uint)digit;
        }

        value = decodedValue;
        return true;
    }

    /// <summary>
    /// Attempts to decode an exactly 12-byte ASCII base-40 value.
    /// </summary>
    /// <returns><see langword="true"/> when decoding succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryDecode(ReadOnlySpan<byte> encodedValue, out ulong value)
    {
        value = default;
        if (encodedValue.Length != UInt64Width)
        {
            return false;
        }

        ulong decodedValue = 0;
        for (int index = 0; index < encodedValue.Length; index++)
        {
            int digit = DecodeDigit(encodedValue[index]);
            if (digit < 0)
            {
                return false;
            }

            decodedValue = decodedValue * Radix + (uint)digit;
        }

        value = decodedValue;
        return true;
    }

    /// <summary>
    /// Attempts to decode an exactly 25-character base-40 value.
    /// </summary>
    /// <returns><see langword="true"/> when decoding succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryDecode(ReadOnlySpan<char> encodedValue, out UInt128 value)
    {
        value = default;
        if (encodedValue.Length != UInt128Width)
        {
            return false;
        }

        UInt128 decodedValue = 0;
        for (int index = 0; index < encodedValue.Length; index++)
        {
            int digit = DecodeDigit(encodedValue[index]);
            if (digit < 0)
            {
                return false;
            }

            UInt128 unsignedDigit = (uint)digit;
            if (decodedValue > (UInt128.MaxValue - unsignedDigit) / Radix)
            {
                return false;
            }

            decodedValue = decodedValue * Radix + unsignedDigit;
        }

        value = decodedValue;
        return true;
    }

    /// <summary>
    /// Attempts to decode an exactly 25-byte ASCII base-40 value.
    /// </summary>
    /// <returns><see langword="true"/> when decoding succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryDecode(ReadOnlySpan<byte> encodedValue, out UInt128 value)
    {
        value = default;
        if (encodedValue.Length != UInt128Width)
        {
            return false;
        }

        UInt128 decodedValue = 0;
        for (int index = 0; index < encodedValue.Length; index++)
        {
            int digit = DecodeDigit(encodedValue[index]);
            if (digit < 0)
            {
                return false;
            }

            UInt128 unsignedDigit = (uint)digit;
            if (decodedValue > (UInt128.MaxValue - unsignedDigit) / Radix)
            {
                return false;
            }

            decodedValue = decodedValue * Radix + unsignedDigit;
        }

        value = decodedValue;
        return true;
    }

    private static int DecodeDigit(char character)
    {
        if (character is >= '0' and <= '9')
        {
            return character - '0';
        }

        if (character is >= 'a' and <= 'z')
        {
            return character - 'a' + 10;
        }

        return character switch
        {
            '-' => 36,
            '_' => 37,
            '.' => 38,
            '~' => 39,
            _ => -1
        };
    }

    private static int DecodeDigit(byte value)
    {
        if (value is >= (byte)'0' and <= (byte)'9')
        {
            return value - '0';
        }

        if (value is >= (byte)'a' and <= (byte)'z')
        {
            return value - 'a' + 10;
        }

        return value switch
        {
            (byte)'-' => 36,
            (byte)'_' => 37,
            (byte)'.' => 38,
            (byte)'~' => 39,
            _ => -1
        };
    }
}
