using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

internal sealed class WindowRenderCoordinator<TRenderContext> :
    RenderCoordinator<TRenderContext>,
    IDisposable
    where TRenderContext : IRenderContext
{
    private readonly IWindowRegistry _windows;
    private readonly string _windowName;
    private readonly GpuDevice _gpuDevice;
    private readonly Func<Window, SwapchainTexture, CommandBuffer, TRenderContext> _contextFactory;
    private bool _disposed;

    internal WindowRenderCoordinator(
        IWindowRegistry windows,
        string windowName,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<TRenderContext>> renderers,
        Func<Window, SwapchainTexture, CommandBuffer, TRenderContext> contextFactory)
        : base(gpuMemorySystem, renderers)
    {
        _windows = windows;
        _windowName = windowName;
        _gpuDevice = gpuDevice;
        _contextFactory = contextFactory;
        _windows.ClaimWindow(windowName);
    }

    protected override bool TryCreateRenderContext(
        [NotNullWhen(true)] out TRenderContext? renderContext)
    {
        if (!_windows.TryGetWindow(_windowName, out Window window))
        {
            renderContext = default;
            return false;
        }

        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!window.TryWaitAndAcquireSwapchainTexture(commandBuffer, out SwapchainTexture swapchainTexture))
        {
            commandBuffer.Dispose();
            renderContext = default;
            return false;
        }

        try
        {
            renderContext = _contextFactory(window, swapchainTexture, commandBuffer);
            return true;
        }
        catch
        {
            commandBuffer.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _windows.ReleaseWindow(_windowName);
    }
}
