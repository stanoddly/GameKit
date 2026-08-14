using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderContext : IRenderContext
{
    public Window Window { get; }
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }
    public virtual Texture ColorTarget => SwapchainTexture;

    public DefaultRenderContext(Window window, SwapchainTexture swapchainTexture, CommandBuffer commandBuffer)
    {
        Window = window;
        SwapchainTexture = swapchainTexture;
        CommandBuffer = commandBuffer;
    }

    public virtual void Dispose()
    {
        CommandBuffer.Submit();
    }
}
