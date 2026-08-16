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
            .UseDefaultRendering(
                new WindowConfig(Size: (640, 480), Title: "Main Window"))
            .UseDefaultRendering(
                SecondaryView,
                new WindowConfig(Size: (480, 360), Title: "Secondary Window"));

        builder.AddSingleton<IRenderer<DefaultRenderContext>>(PrimaryRenderer.Create);
        builder.AddSingleton<IRenderer<DefaultRenderContext>>(SecondaryWindowRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
