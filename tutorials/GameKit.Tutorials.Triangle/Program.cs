using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Triangle;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            //.AddContentFromZipPattern("data*.pak")
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder
            .RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Game" });
        builder.RegisterFunc<TriangleRenderer>(TriangleRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

        int result;
        using (IGameKitApp gameKitApp = builder.Build())
        {
            result = gameKitApp.Run();
        }

        Console.WriteLine("Goodbye!");
        return result;
    }
}