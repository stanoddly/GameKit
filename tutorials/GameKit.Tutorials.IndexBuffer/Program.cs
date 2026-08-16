using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.IndexBuffer;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (1280, 720), Title: "Index Buffer"));

        builder.AddSingleton<IViewRenderer>(IndexBufferRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
