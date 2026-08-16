using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

internal sealed class ViewRenderCoordinator : IRenderCoordinator, IViewScoped
{
    private readonly Window _window;
    private readonly GpuDevice _gpuDevice;
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly ServiceRegistry<IViewRenderer> _renderers;

    public ViewScope ViewScope => _window.ViewScope;

    internal ViewRenderCoordinator(
        Window window,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IViewRenderer> renderers)
    {
        _window = window;
        _gpuDevice = gpuDevice;
        _gpuMemorySystem = gpuMemorySystem;
        _renderers = renderers;
    }

    public void Execute()
    {
        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!_window.TryWaitAndAcquireSwapchainTexture(
                commandBuffer,
                out SwapchainTexture swapchainTexture))
        {
            commandBuffer.Dispose();
            return;
        }

        using ViewRenderContext renderContext = new(_window, swapchainTexture, commandBuffer);
        foreach (IViewRenderer renderer in _renderers)
        {
            if (renderer.ViewScope == ViewScope)
            {
                renderer.Render(renderContext);
            }
        }

        _gpuMemorySystem.Submit();
    }
}
