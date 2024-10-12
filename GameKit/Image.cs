namespace GameKit;

public abstract class Image: IDisposable
{
    public abstract ReadOnlySpan<byte> Data { get; }
    public abstract (int, int) Size { get; }

    public abstract void Dispose();
}
