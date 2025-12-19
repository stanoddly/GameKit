using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderContext: IDisposable
{
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }

    public DefaultRenderContext(SwapchainTexture swapchainTexture, CommandBuffer commandBuffer)
    {
        SwapchainTexture = swapchainTexture;
        CommandBuffer = commandBuffer;
    }

    public virtual void Dispose()
    {
        CommandBuffer.Dispose();
    }
}