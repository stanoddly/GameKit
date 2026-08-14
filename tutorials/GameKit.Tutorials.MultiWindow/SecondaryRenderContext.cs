using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

public sealed class SecondaryRenderContext : IRenderContext
{
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }
    public Texture ColorTarget => SwapchainTexture;

    public SecondaryRenderContext(
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        SwapchainTexture = swapchainTexture;
        CommandBuffer = commandBuffer;
    }

    public void Dispose()
    {
        CommandBuffer.Submit();
    }
}
