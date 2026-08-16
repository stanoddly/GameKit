using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StorageBuffer;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering(new WindowConfig { Size = (800, 600), Title = "Storage Buffer Demo" });

        builder.AddSingleton<StorageBufferRenderer>(StorageBufferRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, StorageBufferRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
