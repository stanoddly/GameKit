namespace GameKit.RenderOrchestration;

internal sealed class RenderPhaseRegistry<TRenderContext>
    where TRenderContext : IRenderContext
{
    private readonly List<IRenderPhase<TRenderContext>?> _renderPhases = new();
    private bool _dirty;

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

        _renderPhases.Add(renderPhase);
        _dirty = true;
    }

    public void Unregister(IRenderPhase<TRenderContext> renderPhase)
    {
        for (int i = 0; i < _renderPhases.Count; i++)
        {
            if (ReferenceEquals(_renderPhases[i], renderPhase))
            {
                _renderPhases[i] = null;
                _dirty = true;
                return;
            }
        }
    }

    public void Render(TRenderContext renderContext)
    {
        Normalize();

        int renderPhaseCount = _renderPhases.Count;
        for (int i = 0; i < renderPhaseCount; i++)
        {
            IRenderPhase<TRenderContext>? renderPhase = _renderPhases[i];
            if (renderPhase == null)
            {
                continue;
            }

            renderPhase.Render(renderContext);
        }
    }

    private void Normalize()
    {
        if (!_dirty)
        {
            return;
        }

        for (int i = _renderPhases.Count - 1; i >= 0; i--)
        {
            if (_renderPhases[i] == null)
            {
                _renderPhases.RemoveAt(i);
            }
        }

        _renderPhases.Sort(static (left, right) => left!.Order.CompareTo(right!.Order));
        _dirty = false;
    }
}
