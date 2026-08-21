using System.Runtime.CompilerServices;
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

    [Test]
    public void BorrowedTexture_DisposeDoesNotDisposeOwner()
    {
        TestTexture owner = new TestTexture();
        BorrowedTexture borrowedTexture = new BorrowedTexture(owner);

        borrowedTexture.Dispose();

        Assert.That(owner.DisposeCalled, Is.False);
    }

    [Test]
    public void BorrowedTexture_WithoutConsumerReference_CanBeCollectedBeforeOwner()
    {
        TestTexture owner = new TestTexture();
        WeakReference<BorrowedTexture> borrowedTexture = CreateBorrowedTextureReference(owner);

        Assert.That(IsAliveAfterCollection(borrowedTexture), Is.False);
        Assert.That(owner.DisposeCalled, Is.False);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<BorrowedTexture> CreateBorrowedTextureReference(Texture owner)
    {
        BorrowedTexture borrowedTexture = new BorrowedTexture(owner);
        return new WeakReference<BorrowedTexture>(borrowedTexture);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsAliveAfterCollection(WeakReference<BorrowedTexture> borrowedTexture)
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        return borrowedTexture.TryGetTarget(out _);
    }
}

internal sealed class TestTexture : Texture
{
    internal bool DisposeCalled { get; private set; }

    internal TestTexture() : base(default, new ShortSize(1, 1), TextureFormat.R8G8B8A8Unorm, 4)
    {
    }

    public override void Dispose()
    {
        DisposeCalled = true;
    }
}
