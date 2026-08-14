using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderContextProvider
    : WindowRenderContextProvider<DefaultWindow, DefaultRenderContext>
{
    public DefaultRenderContextProvider(
        Window<DefaultWindow> window,
        GpuDevice gpuDevice)
        : base(window, gpuDevice)
    {
    }

    protected override DefaultRenderContext CreateRenderContext(
        Window<DefaultWindow> window,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        return new DefaultRenderContext(swapchainTexture, commandBuffer);
    }
}
