using Pixely.App;
using Pixely.RenderOrchestration;

namespace Pixely.Tutorials.DepthOnly;

static class Program
{
    static int Main(string[] args)
    {
        PixelyAppBuilder builder = new PixelyAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering(
                new WindowConfig(Size: (800, 600), Title: "Depth-Only Pipeline Test"));

        builder.AddSingleton<DepthOnlyRenderer>(DepthOnlyRenderer.Create);
        builder.AddAlias<IRenderer<DefaultRenderContext>, DepthOnlyRenderer>();

        using IPixelyApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
