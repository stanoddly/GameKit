using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TextureArray;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (800, 600), Title: "Texture Array Demo"));

        builder.AddSingleton<TextureArrayRenderer>(TextureArrayRenderer.Create);
        builder.AddAlias<IRenderer<RenderContext>, TextureArrayRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
