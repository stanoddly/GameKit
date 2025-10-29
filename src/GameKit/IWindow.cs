using GameKit.Common;
using GameKit.Gpu;

namespace GameKit;

public interface IWindow : IDisposable
{
    uint Id { get; }
    
    ShortSize RenderSizeInPixels { get; }
    
    TextureFormat ColorTargetFormat { get; }
    
    bool TryAcquireSwapchainTexture(CommandBuffer commandBuffer, out SwapchainTexture swapchainTexture);
}