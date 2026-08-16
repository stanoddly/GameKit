using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

static class Program
{
    internal static readonly ViewScope SecondaryView = new(1);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (640, 480), Title: "Main Window"))
            .UseWindowRendering(
                SecondaryView,
                new WindowConfig(Size: (480, 360), Title: "Secondary Window"));

        builder.AddSingleton<IRenderer<RenderContext>>(PrimaryRenderer.Create);
        builder.AddSingleton<IRenderer<RenderContext>>(SecondaryWindowRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
