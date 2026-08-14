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
        ArgumentNullException.ThrowIfNull(factory);

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
        ArgumentNullException.ThrowIfNull(factory);

        ConfigureRenderCoordinator(services, factory);
        return services;
    }

    public static ServiceCollection UseWindowRendering<TRenderContext>(
        this ServiceCollection services,
        string windowName,
        Func<Window, SwapchainTexture, CommandBuffer, TRenderContext> contextFactory)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(windowName);
        ArgumentNullException.ThrowIfNull(contextFactory);

        ConfigureWindowRendering(services, windowName, contextFactory);
        return services;
    }

    public static GameKitAppBuilder UseWindowRendering<TRenderContext>(
        this GameKitAppBuilder builder,
        string windowName,
        Func<Window, SwapchainTexture, CommandBuffer, TRenderContext> contextFactory)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(windowName);
        ArgumentNullException.ThrowIfNull(contextFactory);

        ConfigureWindowRendering(builder, windowName, contextFactory);
        return builder;
    }

    public static GameKitAppBuilder UseDefaultRendering(this GameKitAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ConfigureWindowRendering(
            builder,
            WindowManager.PrimaryWindowName,
            static (window, swapchainTexture, commandBuffer) =>
                new DefaultRenderContext(window, swapchainTexture, commandBuffer));
        return builder;
    }

    public static ServiceCollection UseDefaultRendering(this ServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        ConfigureWindowRendering(
            services,
            WindowManager.PrimaryWindowName,
            static (window, swapchainTexture, commandBuffer) =>
                new DefaultRenderContext(window, swapchainTexture, commandBuffer));
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
        ThrowIfGraphRegistered<TRenderContext>(services);
        services.AddRegistry<IRenderer<TRenderContext>>(
            static (left, right) => left.Order.CompareTo(right.Order));
        services.AddSingleton<RenderCoordinator<TRenderContext>>(provider => factory(
            provider,
            provider.GetRequiredService<ServiceRegistry<IRenderer<TRenderContext>>>()));
        services.AddSingleton<IRenderCoordinator>(static provider =>
            provider.GetRequiredService<RenderCoordinator<TRenderContext>>());
    }

    private static void ConfigureWindowRendering<TRenderContext>(
        ServiceCollection services,
        string windowName,
        Func<Window, SwapchainTexture, CommandBuffer, TRenderContext> contextFactory)
        where TRenderContext : IRenderContext
    {
        ThrowIfGraphRegistered<TRenderContext>(services);
        services.AddRegistry<IRenderer<TRenderContext>>(
            static (left, right) => left.Order.CompareTo(right.Order));
        services.AddSingleton<WindowRenderCoordinator<TRenderContext>>(provider =>
            new WindowRenderCoordinator<TRenderContext>(
                provider.GetRequiredService<WindowManager>(),
                windowName,
                provider.GetRequiredService<GpuDevice>(),
                provider.GetRequiredService<GpuMemorySystem>(),
                provider.GetRequiredService<ServiceRegistry<IRenderer<TRenderContext>>>(),
                contextFactory));
        services.AddSingleton<RenderCoordinator<TRenderContext>>(static provider =>
            provider.GetRequiredService<WindowRenderCoordinator<TRenderContext>>());
        services.AddSingleton<IRenderCoordinator>(static provider =>
            provider.GetRequiredService<WindowRenderCoordinator<TRenderContext>>());
    }

    private static void ThrowIfGraphRegistered<TRenderContext>(ServiceCollection services)
        where TRenderContext : IRenderContext
    {
        if (services.IsRegistered<RenderCoordinator<TRenderContext>>())
        {
            throw new InvalidOperationException(
                $"A rendering graph for {typeof(TRenderContext).Name} is already registered.");
        }
    }
}
