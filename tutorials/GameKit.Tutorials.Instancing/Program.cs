using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Instancing;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (800, 600), Title: "Instancing Demo"));

        builder.AddSingleton<InstancingRenderer>(InstancingRenderer.Create);
        builder.AddAlias<IViewRenderer, InstancingRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
