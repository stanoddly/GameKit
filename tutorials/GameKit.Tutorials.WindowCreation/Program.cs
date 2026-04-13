using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.WindowCreation;

[Module]
partial class WindowCreationApp : GameKitModule, IGameKitDefault
{
    public AppConfig AppConfig { get; } = new() { Size = (1280, 720), Title = "Game" };
    public GameKitConfig GameKitConfig { get; } = new();
    public VirtualFileSystem FileSystem { get; } = new FileSystemBuilder().Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();

    [Singleton]
    public partial NullRenderPhase<DefaultRenderContext> Renderer { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);
}

static class Program
{
    static int Main()
    {
        using WindowCreationApp app = new();
        return ((IGameKitDefault)app).Run();
    }
}
