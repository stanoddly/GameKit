using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.ImageLoading;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (443, 410), Title: "Image Loading Demo"));

        builder.AddSingleton<ImageLoadingRenderer>(ImageLoadingRenderer.Create);
        builder.AddAlias<IRenderer<RenderContext>, ImageLoadingRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
