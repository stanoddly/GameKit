using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using GameKit.VertexShaderOnly;
using Yak;

namespace GameKit.Tutorials.DepthOnly;

[Module]
partial class DepthOnlyApp : GameKitApp, IDefaultRenderContext
{
    public override AppConfig AppConfig { get; } = new() { Size = (800, 600), Title = "Depth-Only Pipeline Test" };
    public override GameKitConfig GameKitConfig { get; } = new();
    public override VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content")
        .AddSourceFileSystem(EmbeddedFileSystem.Create(typeof(GraphicsPipelineBuilderExtensions).Assembly))
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
        return app.Run();
    }
}
