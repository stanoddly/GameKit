using GameKit.App;
using GameKit.Gpu;
using Yak;

namespace GameKit.RenderOrchestration;

[Module]
public interface IDefaultRenderOrchestration<TRenderContext>
    where TRenderContext : IRenderContext
{
    // Consumer-provided: collects render phases via [OnActivate]
    List<IRenderPhase<TRenderContext>> RenderPhases { get; }

    // Consumer-provided: how to acquire a render context each frame
    IRenderContextProvider<TRenderContext> RenderContextProvider { get; }

    [Singleton]
    DefaultRenderManager<TRenderContext> RenderManager { get; }
}
