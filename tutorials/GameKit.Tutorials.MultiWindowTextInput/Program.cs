using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;
using GameKit.Text;

namespace GameKit.Tutorials.MultiWindowTextInput;

static class Program
{
    internal static readonly ViewScope LeftView = new(0);
    internal static readonly ViewScope RightView = new(1);

    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseWindowRendering(
                LeftView,
                new WindowConfig(Size: (520, 300), Title: "Left text input"))
            .UseWindowRendering(
                RightView,
                new WindowConfig(Size: (520, 300), Title: "Right text input"))
            .UsePencuil(LeftView, clearTarget: true)
            .UsePencuil(RightView, clearTarget: true)
            .AddContentFromProjectDirectory("../GameKit.Tutorials.Hotbar/Content");

        builder.AddSingleton<IPencuilView>(provider =>
            new TextInputView(
                LeftView,
                "Left View",
                new TextInputViewModel("left"),
                provider.GetRequiredService<IFontSystem>()));
        builder.AddSingleton<IPencuilView>(provider =>
            new TextInputView(
                RightView,
                "Right View",
                new TextInputViewModel("right"),
                provider.GetRequiredService<IFontSystem>()));

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
