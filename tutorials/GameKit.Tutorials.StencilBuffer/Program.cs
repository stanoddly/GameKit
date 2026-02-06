using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StencilBuffer;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder
            .RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Stencil Buffer" });
        builder.RegisterFunc<StencilBufferRenderer>(StencilBufferRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
