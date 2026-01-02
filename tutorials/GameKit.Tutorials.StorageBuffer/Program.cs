using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StorageBuffer;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager();

        builder
            .RegisterInstance(new AppConfig { Size = (800, 600), Title = "Storage Buffer Demo" });
        builder.RegisterFunc<StorageBufferRenderer>(StorageBufferRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
