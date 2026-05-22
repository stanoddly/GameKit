using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseDefaultRenderManager<TRenderContext>(this GameKitAppBuilder builder) where TRenderContext: IRenderContext
    {
        DefaultRenderManager<TRenderContext>? renderManager = null;
        builder.OnActivated((instance, _) =>
        {
            if (renderManager != null && instance is IRenderPhase<TRenderContext> renderPhase)
            {
                renderManager.Register(renderPhase);
            }
        });
        builder.OnDisposing((instance, _) =>
        {
            if (renderManager != null && instance is IRenderPhase<TRenderContext> renderPhase)
            {
                renderManager.Unregister(renderPhase);
            }
        });
        builder.AddSingleton<IRenderManager>(sp =>
        {
            renderManager = new DefaultRenderManager<TRenderContext>(
                sp.GetRequiredService<GpuMemorySystem>(),
                sp.GetRequiredService<IRenderContextProvider<TRenderContext>>(),
                sp.GetServices<IRenderPhase<TRenderContext>>());
            return renderManager;
        });
        return builder;
    }

    public static GameKitAppBuilder UseDefaultRenderManager(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<IRenderContextProvider<DefaultRenderContext>, DefaultRenderContextProvider>();
        return builder.UseDefaultRenderManager<DefaultRenderContext>();
    }
}
