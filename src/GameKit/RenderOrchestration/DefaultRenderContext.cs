using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderContext : IRenderContext
{
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }
    public virtual Texture ColorTarget => SwapchainTexture;

    public DefaultRenderContext(
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        SwapchainTexture = swapchainTexture;
        CommandBuffer = commandBuffer;
    }

    public virtual void Dispose()
    {
        CommandBuffer.Submit();
    }
}
