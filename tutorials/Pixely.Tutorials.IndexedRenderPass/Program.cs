using Pixely.App;
using Pixely.RenderOrchestration;

namespace Pixely.Tutorials.IndexedRenderPass;

static class Program
{
    static int Main(string[] args)
    {
        PixelyAppBuilder builder = new PixelyAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering(
                new WindowConfig(Size: (1280, 720), Title: "Indexed Render Pass"));

        builder.AddSingleton<IRenderer<DefaultRenderContext>>(IndexedRenderPassRenderer.Create);

        using IPixelyApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
