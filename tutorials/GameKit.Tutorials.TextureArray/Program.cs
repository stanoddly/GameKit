using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TextureArray;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering();

        builder.AddSingleton(new AppConfig { Size = (800, 600), Title = "Texture Array Demo" });
        builder.AddSingleton<TextureArrayRenderer>(TextureArrayRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, TextureArrayRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}