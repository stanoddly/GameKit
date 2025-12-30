using GameKit.App;
using GameKit.RenderOrchestration;
using GameKit.VertexShaderOnly;

namespace GameKit.Tutorials.DepthOnly;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddVertexShaderOnlySupport()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder.RegisterInstance(new AppConfig { Size = (800, 600), Title = "Depth-Only Pipeline Test" });
        builder.RegisterFunc<DepthOnlyRenderer>(DepthOnlyRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
