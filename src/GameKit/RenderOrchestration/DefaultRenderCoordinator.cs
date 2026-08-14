using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

internal sealed class DefaultRenderCoordinator : RenderCoordinator<DefaultRenderContext>
{
    private readonly WindowManager _windowManager;
    private readonly GpuDevice _gpuDevice;

    internal DefaultRenderCoordinator(
        WindowManager windowManager,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<DefaultRenderContext>> renderers)
        : base(gpuMemorySystem, renderers)
    {
        _windowManager = windowManager;
        _gpuDevice = gpuDevice;
    }

    protected override bool TryCreateRenderContext(
        [NotNullWhen(true)] out DefaultRenderContext? renderContext)
    {
        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!_windowManager.PrimaryWindow.TryWaitAndAcquireSwapchainTexture(
                commandBuffer,
                out SwapchainTexture swapchainTexture))
        {
            commandBuffer.Dispose();
            renderContext = null;
            return false;
        }

        renderContext = new DefaultRenderContext(swapchainTexture, commandBuffer);
        return true;
    }
}
