using GameKit.App;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Extension methods for <see cref="GameKitAppBuilder"/> to simplify registration of the default render manager.
/// </summary>
public static class GameKitAppBuilderExtensions
{
    /// <summary>
    /// Registers the <see cref="DefaultRenderManager{TRenderContext}"/> as the <see cref="IRenderManager"/> implementation.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <typeparam name="TRenderContext">The type of the render context.</typeparam>
    public static GameKitAppBuilder UseDefaultRenderManager<TRenderContext>(this GameKitAppBuilder builder) where TRenderContext: IDisposable
    {
        builder.RegisterType<DefaultRenderManager<TRenderContext>>().As<IRenderManager>();
        return builder;
    }
    
    /// <summary>
    /// Registers the <see cref="DefaultRenderManager{DefaultRenderContext}"/> with the default context provider as the <see cref="IRenderManager"/> implementation.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    public static GameKitAppBuilder UseDefaultRenderManager(this GameKitAppBuilder builder)
    {
        builder.RegisterType<DefaultRenderContextProvider>().As<IRenderContextProvider<DefaultRenderContext>>();
        builder.RegisterType<DefaultRenderManager<DefaultRenderContext>>().As<IRenderManager>();
        return builder;
    }
}
