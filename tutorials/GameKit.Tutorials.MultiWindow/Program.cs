using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRenderManager()
            .UseDefaultRenderManager<SecondaryRenderContext>();

        builder.AddSingleton(new AppConfig { Size = (640, 480), Title = "Primary Window" });
        builder.AddWindow<SecondaryWindow>(new WindowOptions(
            Size: new Size<uint>(480, 360),
            Title: "Secondary Window"));

        builder.AddSingleton<IRenderContextProvider<SecondaryRenderContext>, SecondaryRenderContextProvider>();
        builder.AddSingleton<IRenderPhase<DefaultRenderContext>>(PrimaryRenderer.Create);
        builder.AddSingleton<IRenderPhase<SecondaryRenderContext>>(SecondaryWindowRenderer.Create);

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
