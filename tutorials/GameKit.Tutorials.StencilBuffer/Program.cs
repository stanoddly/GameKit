using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.StencilBuffer;

[Module]
partial class StencilBufferApp : GameKitModule, IGameKitDefault
{
    public AppConfig AppConfig { get; } = new() { Size = (1280, 720), Title = "Stencil Buffer" };
    public GameKitConfig GameKitConfig { get; } = new();
    public VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content").Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton<StencilBufferRenderer>, StaticFactory<StencilBufferRenderer>]
    public partial IRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);
}

static class Program
{
    static int Main()
    {
        using StencilBufferApp app = new();
        return ((IGameKitDefault)app).Run();
    }
}
