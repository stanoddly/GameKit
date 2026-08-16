using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StorageBuffer;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (800, 600), Title: "Storage Buffer Demo"));

        builder.AddSingleton<StorageBufferRenderer>(StorageBufferRenderer.Create);
        builder.AddAlias<IRenderer<RenderContext>, StorageBufferRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
