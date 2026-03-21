using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;

namespace GameKit;

public readonly record struct ResolutionChangedEventArgs(ShortSize OldSize, ShortSize NewSize, ulong Timestamp);

public delegate void ResolutionChangedHandler(ResolutionChangedEventArgs eventArgs);

public interface IWindow : IDisposable
{
    uint Id { get; }

    ShortSize RenderSizeInPixels { get; }

    TextureFormat ColorTargetFormat { get; }
    bool MouseGrab { get; set; }
    bool WindowRelativeMouseMode { get; set; }

    event ResolutionChangedHandler? ResolutionChanged;

    bool TryAcquireSwapchainTexture(CommandBuffer commandBuffer, out SwapchainTexture swapchainTexture);

    void SetFullscreenBorderless(bool fullscreen);

    void SetIcon(Image icon);
}