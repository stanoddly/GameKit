using System.Diagnostics.CodeAnalysis;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Provides a default implementation of <see cref="IRenderContextProvider{TRenderContext}"/> that
/// acquires a swapchain texture and a command buffer.
/// </summary>
public class DefaultRenderContextProvider : IRenderContextProvider<DefaultRenderContext>
{
    private readonly Window _window;
    private readonly GpuDevice _gpuDevice;

    public DefaultRenderContextProvider(Window window, GpuDevice gpuDevice)
    {
        _window = window;
        _gpuDevice = gpuDevice;
    }
    
    /// <summary>
    /// Attempts to create and provide a <see cref="DefaultRenderContext"/>.
    /// </summary>
    /// <param name="renderContext">The created render context, or null if unsuccessful.</param>
    /// <returns>True if the render context was created successfully, false otherwise.</returns>
    public bool TryProvide([NotNullWhen(true)] out DefaultRenderContext? renderContext)
    {
        CommandBuffer renderCommandBuffer = _gpuDevice.AcquireCommandBuffer();
        
        if (!_window.TryWaitAndAcquireSwapchainTexture(renderCommandBuffer, out SwapchainTexture swapchainTexture))
        {
            renderContext = null;
            renderCommandBuffer.Dispose();
            return false;
        }

        renderContext = new(swapchainTexture, renderCommandBuffer);

        return true;
    }
}
