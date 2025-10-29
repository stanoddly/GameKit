using System.Numerics;

namespace GameKit;

//[JsonConverter(typeof(SizeJsonConverter))]
public readonly record struct Size<T>(T Width, T Height) where T: unmanaged, INumberBase<T>
{
    public static readonly Size<T> Zero = new(T.Zero, T.Zero);

    public static implicit operator Size<T>((T width, T height) tuple)
    {
        return new Size<T>(tuple.width, tuple.height);
    }

    public static Size<T> operator /(Size<T> size, T n)
    {
        checked
        {
            return new Size<T>(size.Width / n, size.Height / n);
        }
    }

    public static Size<T> operator -(Size<T> a, Size<T> b)
    {
        checked
        {
            return new Size<T>((a.Width - b.Width), (a.Height - b.Height));
        }
    }
}
