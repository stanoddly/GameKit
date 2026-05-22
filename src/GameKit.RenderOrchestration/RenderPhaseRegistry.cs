namespace GameKit.RenderOrchestration;

internal sealed class RenderPhaseRegistry<TRenderContext>
    where TRenderContext : IRenderContext
{
    private readonly List<IRenderPhase<TRenderContext>?> _renderPhases = new();

    public void Register(IRenderPhase<TRenderContext> renderPhase)
    {
        for (int i = 0; i < _renderPhases.Count; i++)
        {
            IRenderPhase<TRenderContext>? existingRenderPhase = _renderPhases[i];
            if (ReferenceEquals(existingRenderPhase, renderPhase))
            {
                return;
            }
        }

        int insertIndex = _renderPhases.Count;
        for (int i = 0; i < _renderPhases.Count; i++)
        {
            IRenderPhase<TRenderContext>? existingRenderPhase = _renderPhases[i];
            if (existingRenderPhase == null)
            {
                continue;
            }

            if (renderPhase.Order < existingRenderPhase.Order)
            {
                insertIndex = i;
                break;
            }
        }

        _renderPhases.Insert(insertIndex, renderPhase);
    }

    public void Unregister(IRenderPhase<TRenderContext> renderPhase)
    {
        for (int i = 0; i < _renderPhases.Count; i++)
        {
            if (ReferenceEquals(_renderPhases[i], renderPhase))
            {
                _renderPhases[i] = null;
                return;
            }
        }
    }

    public void Render(TRenderContext renderContext)
    {
        int renderPhaseCount = _renderPhases.Count;
        bool needsCompaction = false;
        for (int i = 0; i < renderPhaseCount; i++)
        {
            IRenderPhase<TRenderContext>? renderPhase = _renderPhases[i];
            if (renderPhase == null)
            {
                needsCompaction = true;
                continue;
            }

            renderPhase.Render(renderContext);
        }

        if (needsCompaction)
        {
            Compact();
        }
    }

    private void Compact()
    {
        for (int i = _renderPhases.Count - 1; i >= 0; i--)
        {
            if (_renderPhases[i] == null)
            {
                _renderPhases.RemoveAt(i);
            }
        }
    }
}
