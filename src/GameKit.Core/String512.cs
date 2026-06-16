using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace GameKit;

public readonly struct String512 : IEquatable<String512>
{
    private readonly Vector256<byte> _buffer;

    public String512(ReadOnlySpan<char> input)
    {
        if (input.Length == 0)
        {
            _buffer = default;
            return;
        }

        var byteCount = Encoding.UTF8.GetByteCount(input);
        if (byteCount >= Vector256<byte>.Count)
            throw new ArgumentException("Input string is too long when encoded as UTF8", nameof(input));

        // Create a temporary span over our buffer
        Span<byte> bufferSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<Vector256<byte>, byte>(ref Unsafe.AsRef(in _buffer)), Vector256<byte>.Count));
        Encoding.UTF8.GetBytes(input, bufferSpan);
        // Add '\0' as a last character
        bufferSpan[byteCount] = 0;
    }

    public int Length
    {
        get
        {
            ReadOnlySpan<byte> span = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<Vector256<byte>, byte>(ref Unsafe.AsRef(in _buffer)), Vector256<byte>.Count));
            int length = 0;
            while (length < Vector256<byte>.Count && span[length] != 0)
                length++;
            return length;
        }
    }

    public override string ToString()
    {
        if (_buffer.Equals(default))
            return string.Empty;

        ReadOnlySpan<byte> bufferSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<Vector256<byte>, byte>(ref Unsafe.AsRef(in _buffer)), Vector256<byte>.Count));
        
        // We need to ensure we only decode up to the first null terminator
        // since GetString would otherwise process the entire span
        int length = 0;
        while (length < Vector256<byte>.Count && bufferSpan[length] != 0)
            length++;
            
        if (length == 0)
            return string.Empty;
            
        // Now we're sure we're only decoding the bytes before the first null
        return Encoding.UTF8.GetString(bufferSpan.Slice(0, length));
    }

    public bool Equals(String512 other) => _buffer.Equals(other._buffer);

    public override bool Equals(object? obj) => obj is String512 other && Equals(other);

    public override int GetHashCode() => _buffer.GetHashCode();

    public static bool operator ==(String512 left, String512 right) => left._buffer == right._buffer;

    public static bool operator !=(String512 left, String512 right) => left._buffer != right._buffer;
}

public sealed class RefString512EqualityComparer : IEqualityComparer<String512>
{
    public bool Equals(String512 x, String512 y) => 
        Unsafe.As<String512, Vector256<byte>>(ref x) == Unsafe.As<String512, Vector256<byte>>(ref y);

    public int GetHashCode(String512 obj) => 
        Unsafe.As<String512, Vector256<byte>>(ref obj).GetHashCode();
}
