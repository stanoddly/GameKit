using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

internal sealed class WindowRenderCoordinator<TRenderContext> : RenderCoordinator<TRenderContext>
    where TRenderContext : IRenderContext
{
    private readonly Window<TRenderContext> _window;
    private readonly GpuDevice _gpuDevice;
    private readonly Func<Window<TRenderContext>, SwapchainTexture, CommandBuffer, TRenderContext> _contextFactory;

    internal WindowRenderCoordinator(
        Window<TRenderContext> window,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<TRenderContext>> renderers,
        Func<Window<TRenderContext>, SwapchainTexture, CommandBuffer, TRenderContext> contextFactory)
        : base(gpuMemorySystem, renderers)
    {
        _window = window;
        _gpuDevice = gpuDevice;
        _contextFactory = contextFactory;
    }

    protected override bool TryCreateRenderContext(
        [NotNullWhen(true)] out TRenderContext? renderContext)
    {
        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!_window.TryWaitAndAcquireSwapchainTexture(commandBuffer, out SwapchainTexture swapchainTexture))
        {
            commandBuffer.Dispose();
            renderContext = default;
            return false;
        }

        try
        {
            renderContext = _contextFactory(_window, swapchainTexture, commandBuffer);
            return true;
        }
        catch
        {
            commandBuffer.Dispose();
            throw;
        }
    }
}
