using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorial.WindowCreation;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            //.AddContentFromProjectDirectory("_Content")
            .UseDefaultRenderManager();

        builder.RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Game" });
        builder.RegisterType<NullRenderer<DefaultRenderContext>>().As<IRenderer<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
