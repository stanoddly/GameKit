using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.ImageLoading;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(Size: (443, 410), Title: "Image Loading Demo"));
        builder.AddSingleton<ImageLoadingRenderer>(ImageLoadingRenderer.Create);
        builder.AddAlias<IRenderPhase<DefaultRenderContext>, ImageLoadingRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
