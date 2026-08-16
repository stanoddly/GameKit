using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StencilBuffer;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (1280, 720), Title: "Stencil Buffer"));

        builder.AddSingleton<StencilBufferRenderer>(StencilBufferRenderer.Create);
        builder.AddAlias<IViewRenderer, StencilBufferRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
