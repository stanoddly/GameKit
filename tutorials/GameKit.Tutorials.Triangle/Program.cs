using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Triangle;

static class Program
{
    static int Main(string[] args)
    {
        var gameKitAppBuilder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        gameKitAppBuilder
            .RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Game" });
        gameKitAppBuilder.RegisterFunc(TriangleRenderer.Create);

        using IGameKitApp gameKitApp = gameKitAppBuilder.Build();
        return gameKitApp.Run();
    }
}