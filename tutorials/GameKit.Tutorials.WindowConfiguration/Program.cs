using GameKit;
using GameKit.App;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.WindowConfiguration;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .UseDefaultRenderCoordinator();

        builder.AddWindow(new WindowOptions(
            Size: (800, 600),
            Title: "Window Configuration Demo",
            AlwaysOnTop: true));

        builder.AddSingleton<IRenderPhase<DefaultRenderContext>, NullRenderPhase<DefaultRenderContext>>();

        builder.OnStart((Window window, IKeyboardService keyboardService, PlatformInfo platformInfo) =>
        {
            using RawImage icon = CreateIcon(32, 32);
            window.SetIcon(icon);

            Console.WriteLine($"SDL video driver: {platformInfo.SdlVideoDriver ?? "unknown"}");
            Console.WriteLine($"Always on top: {window.AlwaysOnTop}");
            Console.WriteLine($"Always-on-top supported by current SDL video driver: {window.SupportsAlwaysOnTop}");
            if (window.SupportsAlwaysOnTop)
            {
                Console.WriteLine("Press Space to toggle always-on-top.");
            }
            else
            {
                Console.WriteLine("The SDL Wayland backend does not currently apply always-on-top for normal windows.");
                Console.WriteLine("On KDE Wayland, try running with: SDL_VIDEO_DRIVER=x11 dotnet run");
            }

            keyboardService.KeyDown += (Keyboard keyboard, KeyEventArgs eventArgs) =>
            {
                if (eventArgs.Key != VirtualKey.Space)
                {
                    return;
                }

                if (!window.SupportsAlwaysOnTop)
                {
                    Console.WriteLine("Always-on-top is not supported by the current SDL video driver.");
                    eventArgs.Consume();
                    return;
                }

                window.AlwaysOnTop = !window.AlwaysOnTop;
                Console.WriteLine($"Always on top: {window.AlwaysOnTop}");
                eventArgs.Consume();
            };
        });

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }

    static RawImage CreateIcon(int width, int height)
    {
        byte[] pixels = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = (y * width + x) * 4;
                bool isWhite = (x / 4 + y / 4) % 2 == 0;

                pixels[i + 0] = isWhite ? (byte)255 : (byte)100; // R
                pixels[i + 1] = isWhite ? (byte)255 : (byte)100; // G
                pixels[i + 2] = isWhite ? (byte)255 : (byte)200; // B
                pixels[i + 3] = 255;                              // A
            }
        }

        return new RawImage(pixels, new ShortSize((ushort)width, (ushort)height), PixelFormat.Rgba8888);
    }
}
