namespace Pixely.Gpu;

public readonly record struct RenderTarget(Texture ColorTarget, Texture DepthBuffer) : IDisposable
{
    public void Dispose()
    {
        ColorTarget.Dispose();
        DepthBuffer.Dispose();
    }
}
