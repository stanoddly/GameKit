using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.WindowCreation;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            //.AddContentFromProjectDirectory("_Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (1280, 720), Title: "Game"));

        builder.AddSingleton<IViewRenderer>(new NullViewRenderer(ViewScope));

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
