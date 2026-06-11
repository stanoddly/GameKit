using GameKit.App;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderManager<TRenderContext> : IRenderManager
    where TRenderContext: IRenderContext
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly IRenderContextProvider<TRenderContext> _renderContextProvider;
    private readonly RenderPhaseRegistry<TRenderContext> _renderPhaseRegistry;

    internal DefaultRenderManager(
        GpuMemorySystem gpuMemorySystem,
        IRenderContextProvider<TRenderContext> renderContextProvider,
        RenderPhaseRegistry<TRenderContext> renderPhaseRegistry)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderContextProvider = renderContextProvider;
        _renderPhaseRegistry = renderPhaseRegistry;
    }

    public void Execute()
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
