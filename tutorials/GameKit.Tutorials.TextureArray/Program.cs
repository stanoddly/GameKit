using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TextureArray;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(Size: (800, 600), Title: "Texture Array Demo"));
        builder.AddSingleton<TextureArrayRenderer>(TextureArrayRenderer.Create);
        builder.AddAlias<IRenderPhase<DefaultRenderContext>, TextureArrayRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
