using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public sealed class ViewRenderContext : IRenderContext, IViewScoped
{
    public ViewScope ViewScope { get; }
    public Window Window { get; }
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }
    public Texture ColorTarget => SwapchainTexture;

    internal ViewRenderContext(
        Window window,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        ViewScope = window.ViewScope;
        Window = window;
        SwapchainTexture = swapchainTexture;
        CommandBuffer = commandBuffer;
    }

    public void Dispose()
    {
        CommandBuffer.Submit();
    }
}
