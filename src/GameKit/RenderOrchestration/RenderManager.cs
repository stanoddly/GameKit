using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public abstract class RenderManager
{
    public abstract void Execute();
}

public abstract class RenderManager<TRenderContext> : RenderManager
    where TRenderContext : IRenderContext
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly ServiceRegistry<IRenderPhase<TRenderContext>> _renderPhases;

    protected RenderManager(
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderPhase<TRenderContext>> renderPhases)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderPhases = renderPhases;
    }

    public sealed override void Execute()
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
