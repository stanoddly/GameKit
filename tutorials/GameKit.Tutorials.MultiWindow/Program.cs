using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering();

        builder.AddSingleton(new AppConfig { Size = (640, 480), Title = "Primary Window" });
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(PrimaryRenderer.Create);
        builder.AddSingleton<SecondaryWindowRenderer>(SecondaryWindowRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
