using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public interface IRenderCoordinator
{
    void Execute();
}

public abstract class RenderCoordinator<TRenderContext> : IRenderCoordinator
    where TRenderContext : IRenderContext
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly ServiceRegistry<IRenderPhase<TRenderContext>> _renderPhases;

    protected RenderCoordinator(
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderPhase<TRenderContext>> renderPhases)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderPhases = renderPhases;
    }

    public void Execute()
    {
        if (!TryCreateRenderContext(out TRenderContext? renderContext))
        {
            return;
        }

        using (renderContext)
        {
            foreach (IRenderPhase<TRenderContext> renderPhase in _renderPhases)
            {
                renderPhase.Render(renderContext);
            }

            _gpuMemorySystem.Submit();
        }
    }

    protected abstract bool TryCreateRenderContext(
        [NotNullWhen(true)] out TRenderContext? renderContext);
}
