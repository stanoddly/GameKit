using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.ImageLoading;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (443, 410), Title: "Image Loading Demo"));

        builder.AddSingleton<ImageLoadingRenderer>(ImageLoadingRenderer.Create);
        builder.AddAlias<IViewRenderer, ImageLoadingRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
