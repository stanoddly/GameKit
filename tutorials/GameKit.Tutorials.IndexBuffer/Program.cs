using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.IndexBuffer;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(Size: (1280, 720), Title: "Index Buffer"));
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(IndexBufferRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
