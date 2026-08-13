using GameKit.App;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Coordinates rendering across multiple render phases.
/// </summary>
/// <typeparam name="TRenderContext">The type of the render context used by the render phases.</typeparam>
public class DefaultRenderCoordinator<TRenderContext> : RenderCoordinator
    where TRenderContext: IRenderContext
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly IRenderContextProvider<TRenderContext> _renderContextProvider;
    private readonly RenderPhaseRegistry<TRenderContext> _renderPhaseRegistry;

    internal DefaultRenderCoordinator(
        Window window,
        GpuMemorySystem gpuMemorySystem,
        IRenderContextProvider<TRenderContext> renderContextProvider,
        RenderPhaseRegistry<TRenderContext> renderPhaseRegistry)
        : base(window)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderContextProvider = renderContextProvider;
        _renderPhaseRegistry = renderPhaseRegistry;
    }

    /// <summary>
    /// Executes the rendering pipeline for a single frame.
    /// </summary>
    public override void Execute()
    {
        if (!_renderContextProvider.TryProvide(out TRenderContext? renderContext))
        {
            return;
        }

        using (renderContext)
        {
            _renderPhaseRegistry.Render(renderContext);

            // submit all pending changes before renderContext is disposed
            _gpuMemorySystem.Submit();
        }
    }
}
