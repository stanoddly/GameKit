using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorial.WindowCreation;

class Program
{
    static int Main(string[] args)
    {
        var gameKitAppBuilder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            //.AddContentFromProjectDirectory("_Content")
            .UseDefaultRenderManager();

        gameKitAppBuilder
            .RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Game" });

        using IGameKitApp gameKitApp = gameKitAppBuilder.Build();
        return gameKitApp.Run();
    }
}
