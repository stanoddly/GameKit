using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public interface IRenderCoordinator
{
    void Execute();
}

internal sealed class RenderCoordinator : IRenderCoordinator, IViewScoped
{
    private readonly Window _window;
    private readonly GpuDevice _gpuDevice;
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly ServiceRegistry<IRenderer<RenderContext>> _renderers;

    ViewScope IViewScoped.ViewScope => _window.ViewScope;

    internal RenderCoordinator(
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

        using RenderContext renderContext = new(swapchainTexture, commandBuffer);
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

public abstract class RenderCoordinator<TRenderContext> : IRenderCoordinator
    where TRenderContext : IRenderContext
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly ServiceRegistry<IRenderer<TRenderContext>> _renderers;

    protected RenderCoordinator(
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<TRenderContext>> renderers)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderers = renderers;
    }

    public void Execute()
    {
        if (!TryCreateRenderContext(out TRenderContext? renderContext))
        {
            return;
        }

        using (renderContext)
        {
            foreach (IRenderer<TRenderContext> renderer in _renderers)
            {
                renderer.Render(renderContext);
            }

            _gpuMemorySystem.Submit();
        }
    }

    protected abstract bool TryCreateRenderContext(
        [NotNullWhen(true)] out TRenderContext? renderContext);
}
