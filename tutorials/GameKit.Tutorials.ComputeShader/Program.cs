using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.ComputeShader;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (800, 600), Title: "Compute Shader Demo"));

        builder.AddSingleton<ComputeRenderer>(ComputeRenderer.Create);
        builder.AddAlias<IViewRenderer, ComputeRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
