using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

public sealed class SecondaryRenderContextProvider
    : WindowRenderContextProvider<SecondaryWindow, SecondaryRenderContext>
{
    public SecondaryRenderContextProvider(
        SecondaryWindow window,
        GpuDevice gpuDevice)
        : base(window, gpuDevice)
    {
    }

    protected override SecondaryRenderContext CreateRenderContext(
        SecondaryWindow window,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer)
    {
        return new SecondaryRenderContext(swapchainTexture, commandBuffer);
    }
}
