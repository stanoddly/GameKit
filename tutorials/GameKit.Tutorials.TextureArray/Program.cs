using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TextureArray;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (800, 600), Title: "Texture Array Demo"));

        builder.AddSingleton<TextureArrayRenderer>(TextureArrayRenderer.Create);
        builder.AddAlias<IViewRenderer, TextureArrayRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
