using Pixely.Gpu;

namespace Pixely.RenderOrchestration;

public sealed class DefaultRenderContext : IRenderContext
{
    public SwapchainTexture SwapchainTexture { get; }
    public CommandBuffer CommandBuffer { get; }
    public Texture ColorTarget => SwapchainTexture;

    internal DefaultRenderContext(
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
