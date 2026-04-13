using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.Triangle;

[Module]
partial class TriangleApp : GameKitModule, IGameKitDefault
{
    public override AppConfig AppConfig { get; } = new() { Size = (1280, 720), Title = "Game" };
    public override GameKitConfig GameKitConfig { get; } = new();
    public override VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content").Create();

    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton<TriangleRenderer>, StaticFactory<TriangleRenderer>]
    public partial IRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);
}

static class Program
{
    static int Main()
    {
        using TriangleApp app = new();
        return ((IGameKitDefault)app).Run();
    }
}
