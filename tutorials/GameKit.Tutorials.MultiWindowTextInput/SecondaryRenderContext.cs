using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindowTextInput;

public sealed class SecondaryRenderContext : DefaultRenderContext
{
    private SecondaryRenderContext(
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
        : base(swapchainTexture, commandBuffer)
    {
    }

    public static SecondaryRenderContext Create(
        Window<SecondaryRenderContext> _,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        return new SecondaryRenderContext(swapchainTexture, commandBuffer);
    }
}
