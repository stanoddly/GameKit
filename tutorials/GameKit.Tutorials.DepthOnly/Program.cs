using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.DepthOnly;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering(new WindowConfig { Size = (800, 600), Title = "Depth-Only Pipeline Test" });

        builder.AddSingleton<DepthOnlyRenderer>(DepthOnlyRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, DepthOnlyRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
