using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Instancing;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(Size: (800, 600), Title: "Instancing Demo"));
        builder.AddSingleton<InstancingRenderer>(InstancingRenderer.Create);
        builder.AddAlias<IRenderPhase<DefaultRenderContext>, InstancingRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
