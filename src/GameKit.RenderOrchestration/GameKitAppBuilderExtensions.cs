using GameKit.App;

namespace GameKit.RenderOrchestration;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder UseDefaultRenderManager<TRenderContext>(this GameKitAppBuilder builder) where TRenderContext: IDisposable
    {
        builder.RegisterType<DefaultRenderManager<TRenderContext>>().As<IRenderManager>();
        return builder;
    }
    
    public static GameKitAppBuilder UseDefaultRenderManager(this GameKitAppBuilder builder)
    {
        builder.RegisterType<DefaultRenderContextProvider>().As<IRenderContextProvider<DefaultRenderContext>>();
        builder.RegisterType<DefaultRenderManager<DefaultRenderContext>>().As<IRenderManager>();
        return builder;
    }
}
