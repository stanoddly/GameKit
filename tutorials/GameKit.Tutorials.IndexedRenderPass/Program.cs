using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.IndexedRenderPass;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (1280, 720), Title: "Indexed Render Pass"));

        builder.AddSingleton<IViewRenderer>(IndexedRenderPassRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
