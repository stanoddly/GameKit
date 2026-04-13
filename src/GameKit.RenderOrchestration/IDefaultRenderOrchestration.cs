using GameKit.App;
using GameKit.Modules;
using Yak;

namespace GameKit.RenderOrchestration;

[Module]
public interface IDefaultRenderOrchestration<TRenderContext> : IGameKitCore
    where TRenderContext : IRenderContext
{
    // Consumer-provided: collects render phases via [OnActivate]
    List<IRenderPhase<TRenderContext>> RenderPhases { get; }

    // Consumer-provided: how to acquire a render context each frame
    IRenderContextProvider<TRenderContext> RenderContextProvider { get; }

    [Singleton]
    DefaultRenderManager<TRenderContext> RenderManager { get; }
}
