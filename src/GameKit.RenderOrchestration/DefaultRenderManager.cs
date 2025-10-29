using GameKit.App;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public class DefaultRenderManager<TRenderContext> : IRenderManager
    where TRenderContext: IDisposable
{
    private readonly IRenderer<TRenderContext>[] _renderers;
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly IRenderContextProvider<TRenderContext> _renderContextProvider;

    public DefaultRenderManager(GpuMemorySystem gpuMemorySystem, IRenderContextProvider<TRenderContext> renderContextProvider, IEnumerable<IRenderer<TRenderContext>> renderers)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderContextProvider = renderContextProvider;
        _renderers = renderers.OrderBy(r => r.Order).ToArray();
    }

    public void Execute()
    {
        if (!_renderContextProvider.TryProvide(out TRenderContext? renderContext))
        {
            return;
        }

        using (renderContext)
        {
            foreach (IRenderer<TRenderContext> renderer in _renderers)
            {
                renderer.Render(renderContext);
            }
            
            // submit all pending changes before renderContext is disposed
            _gpuMemorySystem.Submit();
        }
    }
}