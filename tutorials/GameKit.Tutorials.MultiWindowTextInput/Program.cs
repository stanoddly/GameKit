using GameKit.App;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindowTextInput;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseWindowRendering<PrimaryRenderContext>(
                new WindowConfig(
                    Size: new Size<uint>(520, 300),
                    Title: "Primary text input"),
                static (_, swapchainTexture, commandBuffer) =>
                    new PrimaryRenderContext(swapchainTexture, commandBuffer))
            .UseWindowRendering<SecondaryRenderContext>(
                new WindowConfig(
                    Size: new Size<uint>(520, 300),
                    Title: "Secondary text input"),
                static (_, swapchainTexture, commandBuffer) =>
                    new SecondaryRenderContext(swapchainTexture, commandBuffer))
            .UsePencuil<PrimaryRenderContext>(clearTarget: true)
            .UsePencuil<SecondaryRenderContext>(clearTarget: true)
            .AddContentFromProjectDirectory("../GameKit.Tutorials.Hotbar/Content");

        builder.AddSingleton<TextInputViewModel<PrimaryRenderContext>>();
        builder.AddSingleton<IView<PrimaryRenderContext>, TextInputView<PrimaryRenderContext>>();
        builder.AddSingleton<TextInputViewModel<SecondaryRenderContext>>();
        builder.AddSingleton<IView<SecondaryRenderContext>, TextInputView<SecondaryRenderContext>>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
