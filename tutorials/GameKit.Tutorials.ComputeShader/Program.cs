using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.ComputeShader;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseWindowRendering(
                new WindowConfig(Size: (800, 600), Title: "Compute Shader Demo"));

        builder.AddSingleton<ComputeRenderer>(ComputeRenderer.Create);
        builder.AddAlias<IRenderer<RenderContext>, ComputeRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
