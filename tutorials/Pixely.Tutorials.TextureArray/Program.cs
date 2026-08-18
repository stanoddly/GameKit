using Pixely.App;
using Pixely.RenderOrchestration;

namespace Pixely.Tutorials.TextureArray;

static class Program
{
    static int Main(string[] args)
    {
        PixelyAppBuilder builder = new PixelyAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering(
                new WindowConfig(Size: (800, 600), Title: "Texture Array Demo"));

        builder.AddSingleton<TextureArrayRenderer>(TextureArrayRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, TextureArrayRenderer>();

        using IPixelyApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
