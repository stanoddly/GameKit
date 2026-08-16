using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.IndexedRenderPass;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (1280, 720), Title: "Indexed Render Pass"));

        builder.AddSingleton<IRenderer<RenderContext>>(IndexedRenderPassRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
