using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TextInput;

static class Program
{
    internal static readonly ViewScope ViewScope = new(0);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseWindowRendering(
                ViewScope,
                new WindowConfig(Size: (640, 440), Title: "Text Input"))
            .UsePencuil(ViewScope)
            .AddContentFromProjectDirectory("../GameKit.Tutorials.Hotbar/Content");

        builder.AddSingleton<TextInputViewModel>();
        builder.AddSingleton<IPencuilView, TextInputView>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
