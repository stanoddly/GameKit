using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.StageSwitching;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRendering()
            .UsePencuil()
            .AddContentFromProjectDirectory("../GameKit.Tutorials.Hotbar/Content");

        builder.AddSingleton(new WindowConfig { Size = (960, 540), Title = "Stage Switching" });
        builder.AddSingleton<IView, MenuView>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
