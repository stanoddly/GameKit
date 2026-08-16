using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Instancing;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (800, 600), Title: "Instancing Demo"));

        builder.AddSingleton<InstancingRenderer>(InstancingRenderer.Create);
        builder.AddAlias<IRenderer<RenderContext>, InstancingRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
