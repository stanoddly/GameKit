using GameKit.DependencyInjection;

namespace GameKit.RenderOrchestration;

internal sealed class RenderPhaseRegistry<TRenderContext>
    where TRenderContext : IRenderContext
{
    private readonly ServiceRegistry<IRenderPhase<TRenderContext>> _renderPhases;
    private readonly List<IRenderPhase<TRenderContext>> _orderedRenderPhases = new();
    private int _registryVersion = -1;

    public RenderPhaseRegistry(ServiceRegistry<IRenderPhase<TRenderContext>> renderPhases)
    {
        _renderPhases = renderPhases;
    }

    public void Render(TRenderContext renderContext)
    {
        RefreshIfNeeded();

        int renderPhaseCount = _orderedRenderPhases.Count;
        for (int i = 0; i < renderPhaseCount; i++)
        {
            IRenderPhase<TRenderContext> renderPhase = _orderedRenderPhases[i];
            renderPhase.Render(renderContext);
        }
    }

    private void RefreshIfNeeded()
    {
        if (_registryVersion == _renderPhases.Version)
        {
            return;
        }

        _orderedRenderPhases.Clear();
        IReadOnlyList<IRenderPhase<TRenderContext>> renderPhases = _renderPhases.Services;
        for (int i = 0; i < renderPhases.Count; i++)
        {
            _orderedRenderPhases.Add(renderPhases[i]);
        }

        _orderedRenderPhases.Sort(static (left, right) => left.Order.CompareTo(right.Order));
        _registryVersion = _renderPhases.Version;
    }
}
