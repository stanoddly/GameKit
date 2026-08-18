using Pixely.App;
using Pixely.Pencuil;
using Pixely.RenderOrchestration;

namespace Pixely.Tutorials.FileDialogs;

static class Program
{
    static int Main(string[] args)
    {
        PixelyAppBuilder builder = new PixelyAppBuilder()
            .UseDefaultRendering(
                new WindowConfig(Size: (960, 540), Title: "File Dialogs"))
            .UsePencuil()
            .AddContentFromProjectDirectory("../Pixely.Tutorials.Hotbar/Content");

        builder.AddSingleton(new FileDialogsViewModel());
        builder.AddSingleton<IPencuilView, FileDialogsView>();

        using IPixelyApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
