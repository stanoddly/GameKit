using Pixely.App;
using Pixely.Pencuil;
using Pixely.RenderOrchestration;

namespace Pixely.Tutorials.StageSwitching;

static class Program
{
    static int Main(string[] args)
    {
        PixelyAppBuilder builder = new PixelyAppBuilder()
            .UseDefaultRendering(
                new WindowConfig(Size: (960, 540), Title: "Stage Switching"))
            .UsePencuil()
            .AddContentFromProjectDirectory("../Pixely.Tutorials.Hotbar/Content");

        builder.AddSingleton<IPencuilView, MenuView>();

        using IPixelyApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
