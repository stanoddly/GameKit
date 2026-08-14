using System.Diagnostics.CodeAnalysis;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public abstract class WindowRenderContextProvider<TWindow, TRenderContext>
    : IRenderContextProvider<TRenderContext>
    where TWindow : class
    where TRenderContext : IRenderContext
{
    private readonly GpuDevice _gpuDevice;
    private readonly Window<TWindow> _window;

    protected WindowRenderContextProvider(
        Window<TWindow> window,
        GpuDevice gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;
    }

    public bool TryProvide([NotNullWhen(true)] out TRenderContext? renderContext)
    {
        if (_window.IsDisposed)
        {
            renderContext = default;
            return false;
        }

        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!_window.TryWaitAndAcquireSwapchainTexture(
                commandBuffer,
                out SwapchainTexture swapchainTexture))
        {
            renderContext = default;
            commandBuffer.Dispose();
            return false;
        }

        try
        {
            renderContext = CreateRenderContext(_window, swapchainTexture, commandBuffer);
            return true;
        }
        catch
        {
            commandBuffer.Dispose();
            throw;
        }
    }

    protected abstract TRenderContext CreateRenderContext(
        Window<TWindow> window,
        SwapchainTexture swapchainTexture,
        CommandBuffer commandBuffer);
}
