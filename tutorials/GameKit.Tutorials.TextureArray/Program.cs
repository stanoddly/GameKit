using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TextureArray;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder
            .RegisterInstance(new AppConfig { Size = (800, 600), Title = "Texture Array Demo" });
        builder.RegisterFunc<TextureArrayRenderer>(TextureArrayRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}