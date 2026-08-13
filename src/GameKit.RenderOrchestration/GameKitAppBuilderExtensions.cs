using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseDefaultRenderCoordinator<TRenderContext>(
        this GameKitAppBuilder builder)
        where TRenderContext : IRenderContext
    {
        ConfigureDefaultRenderCoordinator<TRenderContext>(builder);
        return builder;
    }

    public static ServiceCollection UseDefaultRenderCoordinator<TRenderContext>(
        this ServiceCollection services)
        where TRenderContext : IRenderContext
    {
        ConfigureDefaultRenderCoordinator<TRenderContext>(services);
        return services;
    }

    public static GameKitAppBuilder UseDefaultRenderCoordinator(this GameKitAppBuilder builder)
    {
        ConfigureDefaultRenderContext(builder);
        return builder;
    }

    public static ServiceCollection UseDefaultRenderCoordinator(this ServiceCollection services)
    {
        ConfigureDefaultRenderContext(services);
        return services;
    }

    private static void ConfigureDefaultRenderContext(ServiceCollection services)
    {
        services.AddSingleton<IRenderContextProvider<DefaultRenderContext>, DefaultRenderContextProvider>();
        ConfigureDefaultRenderCoordinator<DefaultRenderContext>(services);
    }

    private static void ConfigureDefaultRenderCoordinator<TRenderContext>(ServiceCollection services)
        where TRenderContext : IRenderContext
    {
        RenderPhaseRegistry<TRenderContext> renderPhaseRegistry = new();
        services.OnActivated((instance, _) =>
        {
            if (instance is IRenderPhase<TRenderContext> renderPhase)
            {
                renderPhaseRegistry.Register(renderPhase);
            }
        });
        services.OnDisposing((instance, _) =>
        {
            if (instance is IRenderPhase<TRenderContext> renderPhase)
            {
                renderPhaseRegistry.Unregister(renderPhase);
            }
        });
        services.AddSingleton<RenderCoordinator>(provider =>
            new DefaultRenderCoordinator<TRenderContext>(
                provider.GetRequiredService<Window>(),
                provider.GetRequiredService<GpuMemorySystem>(),
                provider.GetRequiredService<IRenderContextProvider<TRenderContext>>(),
                renderPhaseRegistry));
    }
}
