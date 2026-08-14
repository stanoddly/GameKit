using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderContextProvider
    : WindowRenderContextProvider<DefaultWindow, DefaultRenderContext>
{
    public DefaultRenderContextProvider(
        DefaultWindow window,
        GpuDevice gpuDevice)
        : base(window, gpuDevice)
    {
    }

    protected override DefaultRenderContext CreateRenderContext(
        DefaultWindow window,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        return new DefaultRenderContext(swapchainTexture, commandBuffer);
    }
}
