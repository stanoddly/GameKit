using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Triangle;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (1280, 720), Title: "Game"));

        builder.AddSingleton<IViewRenderer>(TriangleRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
