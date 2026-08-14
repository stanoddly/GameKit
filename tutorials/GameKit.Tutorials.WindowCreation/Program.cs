using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.WindowCreation;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            //.AddContentFromProjectDirectory("_Content")
            .UseDefaultRendering();

        builder.AddSingleton(new AppConfig { Size = (1280, 720), Title = "Game" });
        builder.AddSingleton<IRenderer<DefaultRenderContext>, NullRenderer<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
