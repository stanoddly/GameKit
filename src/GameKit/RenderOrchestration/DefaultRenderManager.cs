using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

internal sealed class DefaultRenderManager : RenderManager<DefaultRenderContext>
{
    private readonly WindowManager _windowManager;
    private readonly GpuDevice _gpuDevice;

    internal DefaultRenderManager(
        WindowManager windowManager,
        GpuDevice gpuDevice,
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderPhase<DefaultRenderContext>> renderPhases)
        : base(gpuMemorySystem, renderPhases)
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
