using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Hotbar;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .UseDefaultRenderManager();

        builder.RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Hotbar" });
        builder.RegisterType<NullGuiPlatform>().As<IGuiPlatform>();
        builder.RegisterInstance(GuiStyles.Style);
        builder.RegisterType<GuiContext>();
        builder.RegisterType<HotbarRenderer>().As<IRenderPhase<DefaultRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
