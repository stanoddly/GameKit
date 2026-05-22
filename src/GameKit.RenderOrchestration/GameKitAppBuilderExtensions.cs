using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseDefaultRenderManager<TRenderContext>(this GameKitAppBuilder builder) where TRenderContext: IRenderContext
    {
        RenderPhaseRegistry<TRenderContext> renderPhaseRegistry = new();
        builder.OnActivated((instance, _) =>
        {
            if (instance is IRenderPhase<TRenderContext> renderPhase)
            {
                renderPhaseRegistry.Register(renderPhase);
            }
        });
        builder.OnDisposing((instance, _) =>
        {
            if (instance is IRenderPhase<TRenderContext> renderPhase)
            {
                renderPhaseRegistry.Unregister(renderPhase);
            }
        });
        builder.AddSingleton<IRenderManager>(sp => new DefaultRenderManager<TRenderContext>(
            sp.GetRequiredService<GpuMemorySystem>(),
            sp.GetRequiredService<IRenderContextProvider<TRenderContext>>(),
            renderPhaseRegistry));
        return builder;
    }

    public static GameKitAppBuilder UseDefaultRenderManager(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<IRenderContextProvider<DefaultRenderContext>, DefaultRenderContextProvider>();
        return builder.UseDefaultRenderManager<DefaultRenderContext>();
    }
}
