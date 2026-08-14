using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseRenderCoordinator<TRenderContext>(
        this GameKitAppBuilder builder,
        Func<
            ServiceProvider,
            ServiceRegistry<IRenderPhase<TRenderContext>>,
            RenderCoordinator<TRenderContext>> factory)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(factory);

        builder.AddRegistry<IRenderPhase<TRenderContext>>(
            static (left, right) => left.Order.CompareTo(right.Order));
        builder.AddSingleton<IRenderCoordinator>(provider => factory(
            provider,
            provider.GetRequiredService<ServiceRegistry<IRenderPhase<TRenderContext>>>()));
        return builder;
    }

    public static GameKitAppBuilder UseDefaultRendering(this GameKitAppBuilder builder)
    {
        return builder.UseRenderCoordinator<DefaultRenderContext>(
            static (provider, renderPhases) => new DefaultRenderCoordinator(
                provider.GetRequiredService<WindowManager>(),
                provider.GetRequiredService<GpuDevice>(),
                provider.GetRequiredService<GpuMemorySystem>(),
                renderPhases));
    }
}
