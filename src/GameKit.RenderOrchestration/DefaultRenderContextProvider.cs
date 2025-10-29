using System.Diagnostics.CodeAnalysis;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderContextProvider : IRenderContextProvider<DefaultRenderContext>
{
    private readonly IWindow _window;
    private readonly IGpuDevice _gpuDevice;

    public DefaultRenderContextProvider(IWindow window, IGpuDevice gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;
    }
    
    public bool TryProvide([NotNullWhen(true)] out DefaultRenderContext? renderContext)
    {
        CommandBuffer renderCommandBuffer = _gpuDevice.AcquireCommandBuffer();
        
        if (!_window.TryAcquireSwapchainTexture(renderCommandBuffer, out SwapchainTexture swapchainTexture))
        {
            renderContext = null;
            renderCommandBuffer.Dispose();
            return false;
        }

        renderContext = new(swapchainTexture, renderCommandBuffer);

        return true;
    }
}