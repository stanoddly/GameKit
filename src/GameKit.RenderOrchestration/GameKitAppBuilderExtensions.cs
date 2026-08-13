using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseDefaultRenderManager<TRenderContext>(this GameKitAppBuilder builder) where TRenderContext: IRenderContext
    {
        builder.AddRegistry<IRenderPhase<TRenderContext>>(
            static (left, right) => left.Order.CompareTo(right.Order));
        builder.AddSingleton<IRenderManager>(sp => new DefaultRenderManager<TRenderContext>(
            sp.GetRequiredService<GpuMemorySystem>(),
            sp.GetRequiredService<IRenderContextProvider<TRenderContext>>(),
            sp.GetRequiredService<ServiceRegistry<IRenderPhase<TRenderContext>>>()));
        return builder;
    }

    public static GameKitAppBuilder UseDefaultRenderManager(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<IRenderContextProvider<DefaultRenderContext>, DefaultRenderContextProvider>();
        return builder.UseDefaultRenderManager<DefaultRenderContext>();
    }
}
