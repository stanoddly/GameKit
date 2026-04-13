using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.TextureArray;

[Module]
partial class TextureArrayApp : GameKitApp, IDefaultRenderContext
{
    public override AppConfig AppConfig { get; } = new() { Size = (800, 600), Title = "Texture Array Demo" };
    public override GameKitConfig GameKitConfig { get; } = new();
    public override VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content").Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton<TextureArrayRenderer>, StaticFactory<TextureArrayRenderer>]
    public partial IRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);
}

static class Program
{
    static int Main()
    {
        using TextureArrayApp app = new();
        return app.Run();
    }
}
