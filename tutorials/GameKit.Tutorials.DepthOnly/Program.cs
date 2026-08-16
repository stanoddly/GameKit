using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.DepthOnly;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (800, 600), Title: "Depth-Only Pipeline Test"));

        builder.AddSingleton<DepthOnlyRenderer>(DepthOnlyRenderer.Create);
        builder.AddAlias<IViewRenderer, DepthOnlyRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
