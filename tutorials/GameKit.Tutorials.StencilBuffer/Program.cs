using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StencilBuffer;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (1280, 720), Title: "Stencil Buffer"));

        builder.AddSingleton<StencilBufferRenderer>(StencilBufferRenderer.Create);
        builder.AddAlias<IRenderer<RenderContext>, StencilBufferRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
