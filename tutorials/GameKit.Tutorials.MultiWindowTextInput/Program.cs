using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindowTextInput;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRendering(new WindowConfig(
                Size: new Size<uint>(520, 300),
                Title: "Default text input"))
            .UseWindowRendering<SecondaryRenderContext>(
                new WindowConfig(
                    Size: new Size<uint>(520, 300),
                    Title: "Secondary text input"),
                SecondaryRenderContext.Create)
            .UsePencuil()
            .UsePencuil<SecondaryRenderContext>(clearTarget: true)
            .AddContentFromProjectDirectory("../GameKit.Tutorials.Hotbar/Content");

        builder.AddSingleton<TextInputViewModel<DefaultRenderContext>>();
        builder.AddSingleton<IView<DefaultRenderContext>, TextInputView<DefaultRenderContext>>();
        builder.AddSingleton<TextInputViewModel<SecondaryRenderContext>>();
        builder.AddSingleton<IView<SecondaryRenderContext>, TextInputView<SecondaryRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
