using GameKit;
using GameKit.App;
using GameKit.Common;
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

        builder.RegisterInstance(new AppConfig
        {
            Size = (800, 600),
            Title = "Window Configuration Demo"
        });

        builder.RegisterType<NullRenderPhase<DefaultRenderContext>>().As<IRenderPhase<DefaultRenderContext>>();

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

                // Create a simple gradient icon with a border
                bool isBorder = x == 0 || y == 0 || x == width - 1 || y == height - 1;

                if (isBorder)
                {
                    // White border
                    pixels[i + 0] = 255; // R
                    pixels[i + 1] = 255; // G
                    pixels[i + 2] = 255; // B
                    pixels[i + 3] = 255; // A
                }
                else
                {
                    // Blue to purple gradient
                    pixels[i + 0] = (byte)(x * 255 / width);  // R
                    pixels[i + 1] = 100;                       // G
                    pixels[i + 2] = 200;                       // B
                    pixels[i + 3] = 255;                       // A
                }
            }
        }

        return new RawImage(pixels, new ShortSize((ushort)width, (ushort)height), PixelFormat.Rgba8888);
    }
}
