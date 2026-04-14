using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Hotbar;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .UseDefaultRenderManager()
            .UsePencuil()
            .AddContentFromProjectDirectory("Content");

        builder.AddSingleton(new AppConfig { Size = (1280, 720), Title = "Hotbar" });
        builder.AddSingleton(new HotbarViewModel());
        builder.AddSingleton<IView, Hotbar>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
