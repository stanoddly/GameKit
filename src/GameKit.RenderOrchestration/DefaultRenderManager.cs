using GameKit.App;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Manages the overall rendering process by coordinating multiple render phases.
/// </summary>
/// <typeparam name="TRenderContext">The type of the render context used by the render phases.</typeparam>
public class DefaultRenderManager<TRenderContext> : IRenderManager
    where TRenderContext: IRenderContext
{
    private readonly IRenderPhase<TRenderContext>[] _renderPhases;
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly IRenderContextProvider<TRenderContext> _renderContextProvider;

    public DefaultRenderManager(GpuMemorySystem gpuMemorySystem, IRenderContextProvider<TRenderContext> renderContextProvider, List<IRenderPhase<TRenderContext>> renderPhases)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderContextProvider = renderContextProvider;

        if (renderPhases.Count == 0)
        {
            throw new InvalidOperationException($"No instances of {typeof(IRenderPhase<TRenderContext>).FullName} were registered");
        }

        _renderPhases = renderPhases.OrderBy(r => r.Order).ToArray();
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
            foreach (IRenderPhase<TRenderContext> renderer in _renderPhases)
            {
                renderer.Render(renderContext);
            }

            // submit all pending changes before renderContext is disposed
            _gpuMemorySystem.Submit();
        }
    }
}
