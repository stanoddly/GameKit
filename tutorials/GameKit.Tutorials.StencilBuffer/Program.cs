using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StencilBuffer;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering();

        builder.AddSingleton(new AppConfig { Size = (1280, 720), Title = "Stencil Buffer" });
        builder.AddSingleton<StencilBufferRenderer>(StencilBufferRenderer.Create);
        builder.AddAlias<IRenderPhase<DefaultRenderContext>, StencilBufferRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
