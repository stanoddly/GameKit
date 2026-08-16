using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public sealed class RenderContext : IRenderContext, IViewScoped
{
    public Window Window { get; }
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }
    public Texture ColorTarget => SwapchainTexture;

    ViewScope IViewScoped.ViewScope => Window.ViewScope;

    internal RenderContext(
        Window window,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        Window = window;
        SwapchainTexture = swapchainTexture;
        CommandBuffer = commandBuffer;
    }

    public void Dispose()
    {
        CommandBuffer.Submit();
    }
}
