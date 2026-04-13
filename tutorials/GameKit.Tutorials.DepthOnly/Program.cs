using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using GameKit.VertexShaderOnly;
using Yak;

namespace GameKit.Tutorials.DepthOnly;

[Module]
partial class DepthOnlyApp : GameKitModule, IGameKitDefault, IVertexShaderOnly
{
    public AppConfig AppConfig { get; } = new() { Size = (800, 600), Title = "Depth-Only Pipeline Test" };
    public GameKitConfig GameKitConfig { get; } = new();
    public VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content")
        .AddSourceFileSystem(EmbeddedFileSystem.Create(typeof(IVertexShaderOnly).Assembly))
        .Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton<DepthOnlyRenderer>, StaticFactory<DepthOnlyRenderer>]
    public partial IRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);
}

static class Program
{
    static int Main()
    {
        using DepthOnlyApp app = new();
        return ((IGameKitDefault)app).Run();
    }
}
