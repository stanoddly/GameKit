using GameKit.Gpu;

namespace GameKit.Content;

public abstract class Image: IDisposable
{
    public abstract ReadOnlySpan<byte> Data { get; }
    public abstract ShortSize Size { get; }
    public abstract PixelFormat PixelFormat { get; }

    public abstract void Dispose();
}

public class RawImage: Image
{
    private readonly byte[] _data;
    public override ReadOnlySpan<byte> Data => _data;
    public override ShortSize Size { get; }

    public override PixelFormat PixelFormat { get; }

    public RawImage(byte[] data, ShortSize size, PixelFormat pixelFormat)
    {
        _data = data;
        Size = size;
        PixelFormat = pixelFormat;
    }

    

    public override void Dispose() { }
}