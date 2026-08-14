// Requires a patched SDL3 build with the SDL_WINDOW_TRANSPARENT guard removed
// from SDL_ClaimWindowForGPUDevice() in src/gpu/SDL_gpu.c.
// The Vulkan backend supports transparent swapchains natively, but SDL3 blocks
// it at the API level because D3D12 does not support it yet.
// See: https://github.com/libsdl-org/SDL/issues/12410

using GameKit.App;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.TransparentWindow;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering();

        builder.AddSingleton(new AppConfig
        {
            Size = (800, 600),
            Title = "Transparent Window",
            Transparent = true,
            Borderless = true,
            ClearColor = FColors.Transparent
        });
        if (OperatingSystem.IsWindows())
        {
            builder.AddSingleton(new GameKitConfig(GpuBackend: GpuBackend.Vulkan));
        }
        builder.AddSingleton<IRenderer<DefaultRenderContext>>(TransparentWindowRenderer.Create);

        builder.OnStart((IMouseService mouseService, AppControl appControl) =>
        {
            mouseService.ButtonPress += (Mouse mouse, MouseButtonEventArgs eventArgs) => appControl.Quit();
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
