using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public static class RenderingExtensions
{
    public static GameKitAppBuilder UseRenderCoordinator<TRenderContext>(
        this GameKitAppBuilder builder,
        Func<
            ServiceProvider,
            ServiceRegistry<IRenderer<TRenderContext>>,
            RenderCoordinator<TRenderContext>> factory)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ConfigureRenderCoordinator(builder, factory);
        return builder;
    }

    public static ServiceCollection UseRenderCoordinator<TRenderContext>(
        this ServiceCollection services,
        Func<
            ServiceProvider,
            ServiceRegistry<IRenderer<TRenderContext>>,
            RenderCoordinator<TRenderContext>> factory)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ConfigureRenderCoordinator(services, factory);
        return services;
    }

    public static GameKitAppBuilder UseWindowRendering(
        this GameKitAppBuilder builder,
        ViewScope viewScope,
        WindowConfig? config = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ConfigureWindowRendering(builder, viewScope, config ?? new WindowConfig());
        return builder;
    }

    public static ServiceCollection UseWindowRendering(
        this ServiceCollection services,
        ViewScope viewScope,
        WindowConfig? config = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ConfigureWindowRendering(services, viewScope, config ?? new WindowConfig());
        return services;
    }

    private static void ConfigureRenderCoordinator<TRenderContext>(
        ServiceCollection services,
        Func<
            ServiceProvider,
            ServiceRegistry<IRenderer<TRenderContext>>,
            RenderCoordinator<TRenderContext>> factory)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(factory);
        services.AddRegistry<IRenderer<TRenderContext>>(
            static (left, right) => left.Order.CompareTo(right.Order));
        services.AddSingleton<IRenderCoordinator>(provider => factory(
            provider,
            provider.GetRequiredService<ServiceRegistry<IRenderer<TRenderContext>>>()));
    }

    private static void ConfigureWindowRendering(
        ServiceCollection services,
        ViewScope viewScope,
        WindowConfig config)
    {
        if (viewScope.Value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(viewScope),
                viewScope,
                "A window ViewScope must be non-negative.");
        }

        services.AddSingleton<Window>(provider =>
            provider.GetRequiredService<GameKitFactory>().CreateWindow(
                viewScope,
                provider.GetRequiredService<GpuDevice>(),
                provider.GetRequiredService<GameKitFrameContext>(),
                config,
                provider.GetRequiredService<PlatformInfo>()));
        services.AddSingleton<IRenderCoordinator>(provider =>
            new ViewRenderCoordinator(
                provider.GetRequiredService<WindowRegistry>().GetWindow(viewScope),
                provider.GetRequiredService<GpuDevice>(),
                provider.GetRequiredService<GpuMemorySystem>(),
                provider.GetRequiredService<ServiceRegistry<IViewRenderer>>()));
    }
}
