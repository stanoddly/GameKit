using Pixely.DependencyInjection;
using Pixely.Gpu;

namespace Pixely.RenderOrchestration;

internal sealed class DefaultRenderCoordinator : IRenderCoordinator
{
    private readonly Window _window;
    private readonly GpuDevice _gpuDevice;
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly ServiceRegistry<IRenderer<DefaultRenderContext>> _renderers;

    internal DefaultRenderCoordinator(
        Window window,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<DefaultRenderContext>> renderers)
    {
        _window = window;
        _gpuDevice = gpuDevice;
        _gpuMemorySystem = gpuMemorySystem;
        _renderers = renderers;
    }

    public void Execute()
    {
        if (!_window.IsVisible)
        {
            return;
        }

        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!_window.TryWaitAndAcquireSwapchainTexture(
                commandBuffer,
                out SwapchainTexture swapchainTexture))
        {
            commandBuffer.Dispose();
            return;
        }

        using DefaultRenderContext renderContext = new(swapchainTexture, commandBuffer);
        foreach (IRenderer<DefaultRenderContext> renderer in _renderers)
        {
            if (renderer.ViewScope == _window.ViewScope)
            {
                renderer.Render(renderContext);
            }
        }

        _gpuMemorySystem.Submit();
    }
}
