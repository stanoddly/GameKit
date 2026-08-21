using Pixely.Gpu;

namespace Pixely.Tests;

public class TextureTests
{
    [Test]
    public void ThrowIfDisposed_WithNullNativeTexture_ThrowsObjectDisposedException()
    {
        Texture texture = new TestTexture();

        Assert.Throws<ObjectDisposedException>(texture.ThrowIfDisposed);
    }
}

internal sealed class TestTexture : Texture
{
    internal TestTexture() : base(default, new ShortSize(1, 1), TextureFormat.R8G8B8A8Unorm, 4)
    {
    }

    public override void Dispose()
    {
    }
}
