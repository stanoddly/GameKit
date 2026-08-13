using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.IndexedRenderPass;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(Size: (1280, 720), Title: "Indexed Render Pass"));
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(IndexedRenderPassRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
