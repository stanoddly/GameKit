using GameKit.Gpu;
using Yak;

namespace GameKit.RenderOrchestration;

[Module]
public interface IDefaultRenderContext : IDefaultRenderOrchestration<DefaultRenderContext>
{
    [Singleton<DefaultRenderContextProvider>]
    new IRenderContextProvider<DefaultRenderContext> RenderContextProvider { get; }
}
