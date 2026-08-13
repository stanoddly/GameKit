using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Triangle;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(Size: (1280, 720), Title: "Game"));
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(TriangleRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
