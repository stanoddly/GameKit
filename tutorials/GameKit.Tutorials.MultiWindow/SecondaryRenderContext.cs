using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

public sealed class SecondaryRenderContext : IRenderContext
{
    public Window Window { get; }
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }
    public Texture ColorTarget => SwapchainTexture;

    private SecondaryRenderContext(
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

    public static SecondaryRenderContext Create(
        Window window,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        return new SecondaryRenderContext(window, swapchainTexture, commandBuffer);
    }
}
