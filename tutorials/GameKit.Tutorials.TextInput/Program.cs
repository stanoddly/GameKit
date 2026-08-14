using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TextInput;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRendering()
            .UsePencuil()
            .AddContentFromProjectDirectory("../GameKit.Tutorials.Hotbar/Content");

        builder.AddSingleton(new WindowConfig { Size = (640, 440), Title = "Text Input" });
        builder.AddSingleton<TextInputViewModel>();
        builder.AddSingleton<IView, TextInputView>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
