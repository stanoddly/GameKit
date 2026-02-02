using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.ImageLoading;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder
            .RegisterInstance(new AppConfig { Size = (800, 600), Title = "Image Loading Demo" });
        builder.RegisterFunc<ImageLoadingRenderer>(ImageLoadingRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
