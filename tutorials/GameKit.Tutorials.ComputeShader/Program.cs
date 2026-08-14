using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.ComputeShader;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering();

        builder.AddSingleton(new AppConfig { Size = (800, 600), Title = "Compute Shader Demo" });
        builder.AddSingleton<ComputeRenderer>(ComputeRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, ComputeRenderer>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
