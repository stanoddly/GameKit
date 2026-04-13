using GameKit;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.WindowConfiguration;

[Module]
partial class WindowConfigApp : GameKitModule, IDefaultRenderContext
{
    public override AppConfig AppConfig { get; } = new() { Size = (800, 600), Title = "Window Configuration Demo" };
    public override GameKitConfig GameKitConfig { get; } = new();
    public override VirtualFileSystem FileSystem { get; } = new FileSystemBuilder().Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton]
    public partial NullRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);

    [OnActivate]
    void SetWindowIcon(IWindow window)
    {
        using RawImage icon = CreateIcon(32, 32);
        window.SetIcon(icon);
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

                pixels[i + 0] = isWhite ? (byte)255 : (byte)100;
                pixels[i + 1] = isWhite ? (byte)255 : (byte)100;
                pixels[i + 2] = isWhite ? (byte)255 : (byte)200;
                pixels[i + 3] = 255;
            }
        }

        return new RawImage(pixels, new ShortSize((ushort)width, (ushort)height), PixelFormat.Rgba8888);
    }
}

static class Program
{
    static int Main()
    {
        using WindowConfigApp app = new();
        return app.Run();
    }
}
