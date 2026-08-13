using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.FileDialogs;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRenderCoordinator()
            .UsePencuil()
            .AddContentFromProjectDirectory("../GameKit.Tutorials.Hotbar/Content");

        builder.AddWindow(new WindowOptions(Size: (960, 540), Title: "File Dialogs"));
        builder.AddSingleton(new FileDialogsViewModel());
        builder.AddSingleton<IView, FileDialogsView>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
