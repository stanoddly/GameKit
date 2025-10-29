using System.Numerics;

namespace GameKit.Common;

//[JsonConverter(typeof(PointJsonConverter))]
public readonly record struct Location(short X, short Y)
{
    public static readonly Location Zero = new(0, 0);
    public static readonly Location MinValue = new(short.MinValue, short.MinValue);

    public static implicit operator Location((short x, short y) tuple)
    {
        return new Location(tuple.x, tuple.y);
    }
    
    public static implicit operator (short, short)(Location location)
    {
        return new ValueTuple<short, short>(location.X, location.Y);
    }

    public static implicit operator (int, int)(Location location)
    {
        return new ValueTuple<int, int>(location.X, location.Y);
    }

    public static explicit operator Vector2(Location location)
    {
        return new Vector2(location.X, location.Y);
    }

    public static explicit operator Location(Vector2 vector2)
    {
        return new Location((short)vector2.X, (short)vector2.Y);
    }

    public static Location operator +(Location a, Location b)
    {
        checked
        {
            return new Location((short)(a.X + b.X), (short)(a.Y + b.Y));
        }
    }

    public static Location operator -(Location a, Location b)
    {
        checked
        {
            return new Location((short)(a.X + b.X), (short)(a.Y + b.Y));
        }
    }
}
