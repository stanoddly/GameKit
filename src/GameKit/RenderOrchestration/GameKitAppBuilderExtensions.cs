using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseRenderManager<TRenderContext>(
        this GameKitAppBuilder builder,
        Func<
            ServiceProvider,
            ServiceRegistry<IRenderPhase<TRenderContext>>,
            RenderManager<TRenderContext>> factory)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(factory);

        builder.AddRegistry<IRenderPhase<TRenderContext>>(
            static (left, right) => left.Order.CompareTo(right.Order));
        builder.AddSingleton<RenderManager>(provider => factory(
            provider,
            provider.GetRequiredService<ServiceRegistry<IRenderPhase<TRenderContext>>>()));
        return builder;
    }

    public static GameKitAppBuilder UseDefaultRenderManager(this GameKitAppBuilder builder)
    {
        return builder.UseRenderManager<DefaultRenderContext>(
            static (provider, renderPhases) => new DefaultRenderManager(
                provider.GetRequiredService<WindowManager>(),
                provider.GetRequiredService<GpuDevice>(),
                provider.GetRequiredService<GpuMemorySystem>(),
                renderPhases));
    }
}
