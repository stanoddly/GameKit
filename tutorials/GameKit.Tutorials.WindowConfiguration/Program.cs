using GameKit;
using GameKit.App;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.WindowConfiguration;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .UseDefaultRenderManager();

        builder.AddSingleton(new AppConfig
        {
            Size = (800, 600),
            Title = "Window Configuration Demo"
        });

        builder.AddSingleton<IRenderPhase<DefaultRenderContext>, NullRenderPhase<DefaultRenderContext>>();

        builder.OnStart((IWindow window) =>
        {
            using var icon = CreateIcon(32, 32);
            window.SetIcon(icon);
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
