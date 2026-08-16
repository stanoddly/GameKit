using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StorageBuffer;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (800, 600), Title: "Storage Buffer Demo"));

        builder.AddSingleton<StorageBufferRenderer>(StorageBufferRenderer.Create);
        builder.AddAlias<IViewRenderer, StorageBufferRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
