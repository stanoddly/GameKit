using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.IndexedRenderPass;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder.AddSingleton(new AppConfig { Size = (1280, 720), Title = "Indexed Render Pass" });
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(IndexedRenderPassRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
