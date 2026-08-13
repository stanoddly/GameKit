using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Manages the overall rendering process by coordinating multiple render phases.
/// </summary>
/// <typeparam name="TRenderContext">The type of the render context used by the render phases.</typeparam>
public class DefaultRenderManager<TRenderContext> : IRenderManager
    where TRenderContext: IRenderContext
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly IRenderContextProvider<TRenderContext> _renderContextProvider;
    private readonly ServiceRegistry<IRenderPhase<TRenderContext>> _renderPhases;

    internal DefaultRenderManager(
        GpuMemorySystem gpuMemorySystem,
        IRenderContextProvider<TRenderContext> renderContextProvider,
        ServiceRegistry<IRenderPhase<TRenderContext>> renderPhases)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderContextProvider = renderContextProvider;
        _renderPhases = renderPhases;
    }

    /// <summary>
    /// Executes the rendering pipeline for a single frame.
    /// </summary>
    public void Execute()
    {
        if (!_renderContextProvider.TryProvide(out TRenderContext? renderContext))
        {
            return;
        }

        using (renderContext)
        {
            foreach (IRenderPhase<TRenderContext> renderPhase in _renderPhases)
            {
                renderPhase.Render(renderContext);
            }
            
            // submit all pending changes before renderContext is disposed
            _gpuMemorySystem.Submit();
        }
    }
}
