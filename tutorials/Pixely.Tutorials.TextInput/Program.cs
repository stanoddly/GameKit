using Pixely.App;
using Pixely.Pencuil;
using Pixely.RenderOrchestration;

namespace Pixely.Tutorials.TextInput;

static class Program
{
    static int Main(string[] args)
    {
        PixelyAppBuilder builder = new PixelyAppBuilder()
            .UseDefaultRendering(
                new WindowConfig(Size: (640, 440), Title: "Text Input"))
            .UsePencuil()
            .AddContentFromProjectDirectory("../Pixely.Tutorials.Hotbar/Content");

        builder.AddSingleton<TextInputViewModel>();
        builder.AddSingleton<IPencuilView, TextInputView>();

        using IPixelyApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
