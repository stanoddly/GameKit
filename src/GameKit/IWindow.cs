using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;

namespace GameKit;

public interface IWindow : IDisposable
{
    uint Id { get; }

    ShortSize RenderSizeInPixels { get; }

    TextureFormat ColorTargetFormat { get; }
    bool WindowRelativeMouseMode { get; set; }

    bool TryAcquireSwapchainTexture(CommandBuffer commandBuffer, out SwapchainTexture swapchainTexture);

    void SetFullscreenBorderless(bool fullscreen);

    void SetIcon(Image icon);
}