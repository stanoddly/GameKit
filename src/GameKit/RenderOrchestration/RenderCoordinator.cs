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
    private readonly ServiceRegistry<IRenderer<TRenderContext>> _renderers;

    protected RenderCoordinator(
        GpuMemorySystem gpuMemorySystem,
        ServiceRegistry<IRenderer<TRenderContext>> renderers)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _renderers = renderers;
    }

    public void Execute()
    {
        if (!TryCreateRenderContext(out TRenderContext? renderContext))
        {
            return;
        }

        using (renderContext)
        {
            foreach (IRenderer<TRenderContext> renderer in _renderers)
            {
                renderer.Render(renderContext);
            }

            _gpuMemorySystem.Submit();
        }
    }

    protected abstract bool TryCreateRenderContext(
        [NotNullWhen(true)] out TRenderContext? renderContext);
}
