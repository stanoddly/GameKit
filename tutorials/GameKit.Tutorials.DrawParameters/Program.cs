using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.DrawParameters;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder
            .RegisterInstance(new AppConfig { Size = (800, 600), Title = "Draw Parameters Demo" });
        builder.RegisterFunc<DrawParametersRenderer>(DrawParametersRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
