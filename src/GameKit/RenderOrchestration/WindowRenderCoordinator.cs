using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

internal sealed class WindowRenderCoordinator : IRenderCoordinator, IViewScoped
{
    private readonly Window _window;
    private readonly GpuDevice _gpuDevice;
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly ServiceRegistry<IRenderer<RenderContext>> _renderers;

    ViewScope IViewScoped.ViewScope => _window.ViewScope;

    internal WindowRenderCoordinator(
        Window window,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<RenderContext>> renderers)
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

        using RenderContext renderContext = new(_window, swapchainTexture, commandBuffer);
        foreach (IRenderer<RenderContext> renderer in _renderers)
        {
            if (renderer.ViewScope == _window.ViewScope)
            {
                renderer.Render(renderContext);
            }
        }

        _gpuMemorySystem.Submit();
    }
}
