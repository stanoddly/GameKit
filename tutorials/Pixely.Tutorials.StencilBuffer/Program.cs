using Pixely.App;
using Pixely.RenderOrchestration;

namespace Pixely.Tutorials.StencilBuffer;

static class Program
{
    static int Main(string[] args)
    {
        PixelyAppBuilder builder = new PixelyAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering(
                new WindowConfig(Size: (1280, 720), Title: "Stencil Buffer"));

        builder.AddSingleton<StencilBufferRenderer>(StencilBufferRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, StencilBufferRenderer>();

        using IPixelyApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
