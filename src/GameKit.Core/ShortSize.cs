using System.Numerics;

namespace GameKit;

public readonly record struct ShortSize(ushort Width, ushort Height)
{
    public static readonly ShortSize Zero = default;

    public static implicit operator ShortSize((ushort width, ushort height) tuple)
    {
        return new ShortSize(tuple.width, tuple.height);
    }
    
    public static implicit operator (ushort width, ushort height)(ShortSize tuple)
    {
        return (tuple.Width, tuple.Height);
    }
    
    public static implicit operator (uint width, uint height)(ShortSize tuple)
    {
        return (tuple.Width, tuple.Height);
    }
    
    public static implicit operator ShortVector2(ShortSize tuple)
    {
        return new ShortVector2((short)tuple.Width, (short)tuple.Height);
    }

    public static implicit operator UShortVector2(ShortSize tuple)
    {
        return new UShortVector2(tuple.Width, tuple.Height);
    }
    
    public static explicit operator Vector2(ShortSize tuple)
    {
        return new Vector2(tuple.Width, tuple.Height);
    }

    public static ShortSize operator /(ShortSize size, ushort n)
    {
        checked
        {
            return new ShortSize((ushort)(size.Width / n), (ushort)(size.Height / n));
        }
    }

    public static ShortSize operator -(ShortSize a, ShortSize b)
    {
        checked
        {
            return new ShortSize((ushort)(a.Width - b.Width), (ushort)(a.Height - b.Height));
        }
    }
}
