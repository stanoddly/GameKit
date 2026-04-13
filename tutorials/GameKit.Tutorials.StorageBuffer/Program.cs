using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.StorageBuffer;

[Module]
partial class StorageBufferApp : GameKitModule, IGameKitDefault
{
    public AppConfig AppConfig { get; } = new() { Size = (800, 600), Title = "Storage Buffer Demo" };
    public GameKitConfig GameKitConfig { get; } = new();
    public VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content").Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton<StorageBufferRenderer>, StaticFactory<StorageBufferRenderer>]
    public partial IRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);
}

static class Program
{
    static int Main()
    {
        using StorageBufferApp app = new();
        return ((IGameKitDefault)app).Run();
    }
}
