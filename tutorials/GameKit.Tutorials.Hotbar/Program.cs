using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.Hotbar;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .UseDefaultRendering(new WindowConfig { Size = (1280, 720), Title = "Hotbar" })
            .UsePencuil()
            .AddContentFromProjectDirectory("Content");

        builder.AddSingleton(new HotbarViewModel());
        builder.AddSingleton<IView, Hotbar>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
