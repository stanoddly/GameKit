using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

static class Program
{
    internal static readonly ViewScope MainView = new(0);
    internal static readonly ViewScope SecondaryView = new(1);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                MainView,
                new WindowConfig(Size: (640, 480), Title: "Main Window"))
            .UseWindowRendering(
                SecondaryView,
                new WindowConfig(Size: (480, 360), Title: "Secondary Window"));

        builder.AddSingleton<IViewRenderer>(PrimaryRenderer.Create);
        builder.AddSingleton<IViewRenderer>(SecondaryWindowRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
