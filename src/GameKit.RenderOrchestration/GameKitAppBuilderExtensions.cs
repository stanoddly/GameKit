using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseDefaultRenderManager<TRenderContext>(this GameKitAppBuilder builder) where TRenderContext: IRenderContext
    {
        builder.AddSingleton<IRenderManager>(sp => new DefaultRenderManager<TRenderContext>(
            sp.GetService<GpuMemorySystem>(),
            sp.GetService<IRenderContextProvider<TRenderContext>>(),
            sp.GetServices<IRenderPhase<TRenderContext>>()));
        return builder;
    }

    public static GameKitAppBuilder UseDefaultRenderManager(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<IRenderContextProvider<DefaultRenderContext>, DefaultRenderContextProvider>();
        builder.AddSingleton<IRenderManager>(sp => new DefaultRenderManager<DefaultRenderContext>(
            sp.GetService<GpuMemorySystem>(),
            sp.GetService<IRenderContextProvider<DefaultRenderContext>>(),
            sp.GetServices<IRenderPhase<DefaultRenderContext>>()));
        return builder;
    }
}
