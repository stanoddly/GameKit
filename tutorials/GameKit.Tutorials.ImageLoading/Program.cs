using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.ImageLoading;

[Module]
partial class ImageLoadingApp : GameKitApp, IDefaultRenderContext
{
    public override AppConfig AppConfig { get; } = new() { Size = (443, 410), Title = "Image Loading Demo" };
    public override GameKitConfig GameKitConfig { get; } = new();
    public override VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content").Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton<ImageLoadingRenderer>, StaticFactory<ImageLoadingRenderer>]
    public partial IRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);
}

static class Program
{
    static int Main()
    {
        using ImageLoadingApp app = new();
        return app.Run();
    }
}
