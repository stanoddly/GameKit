using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindowTextInput;

public sealed class PrimaryRenderContext : DefaultRenderContext
{
    public PrimaryRenderContext(
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
        : base(swapchainTexture, commandBuffer)
    {
    }
}
