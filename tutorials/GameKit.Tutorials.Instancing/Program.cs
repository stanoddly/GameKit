using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Instancing;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering();

        builder.AddSingleton(new AppConfig { Size = (800, 600), Title = "Instancing Demo" });
        builder.AddSingleton<InstancingRenderer>(InstancingRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, InstancingRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
