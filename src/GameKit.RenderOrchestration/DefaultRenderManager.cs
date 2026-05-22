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
    private readonly List<IRenderPhase<TRenderContext>?> _renderers = new();
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly IRenderContextProvider<TRenderContext> _renderContextProvider;

    public DefaultRenderManager(GpuMemorySystem gpuMemorySystem, IRenderContextProvider<TRenderContext> renderContextProvider, IEnumerable<IRenderPhase<TRenderContext>> renderers)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderContextProvider = renderContextProvider;

        foreach (IRenderPhase<TRenderContext> renderer in renderers)
        {
            Register(renderer);
        }
    }

    public void Register(IRenderPhase<TRenderContext> renderer)
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            IRenderPhase<TRenderContext>? existingRenderer = _renderers[i];
            if (ReferenceEquals(existingRenderer, renderer))
            {
                return;
            }
        }

        int insertIndex = _renderers.Count;
        for (int i = 0; i < _renderers.Count; i++)
        {
            IRenderPhase<TRenderContext>? existingRenderer = _renderers[i];
            if (existingRenderer == null)
            {
                continue;
            }

            if (renderer.Order < existingRenderer.Order)
            {
                insertIndex = i;
                break;
            }
        }

        _renderers.Insert(insertIndex, renderer);
    }

    public void Unregister(IRenderPhase<TRenderContext> renderer)
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            if (ReferenceEquals(_renderers[i], renderer))
            {
                _renderers[i] = null;
                return;
            }
        }
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
            int rendererCount = _renderers.Count;
            bool needsCompaction = false;
            for (int i = 0; i < rendererCount; i++)
            {
                IRenderPhase<TRenderContext>? renderer = _renderers[i];
                if (renderer == null)
                {
                    needsCompaction = true;
                    continue;
                }

                renderer.Render(renderContext);
            }

            if (needsCompaction)
            {
                Compact();
            }
            
            // submit all pending changes before renderContext is disposed
            _gpuMemorySystem.Submit();
        }
    }

    private void Compact()
    {
        for (int i = _renderers.Count - 1; i >= 0; i--)
        {
            if (_renderers[i] == null)
            {
                _renderers.RemoveAt(i);
            }
        }
    }
}
