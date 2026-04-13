using GameKit;
using GameKit.Content;
using GameKit.Modules;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Tutorials.Hotbar;

[Module]
partial class HotbarApp : GameKitModule, IDefaultRenderContext, IPencuil<DefaultRenderContext>
{
    public override AppConfig AppConfig { get; } = new() { Size = (1280, 720), Title = "Hotbar" };
    public override GameKitConfig GameKitConfig { get; } = new();
    public override VirtualFileSystem FileSystem { get; } = new FileSystemBuilder()
        .AddContentFromProjectDirectory("Content")
        .AddSourceFileSystem(EmbeddedFileSystem.Create(typeof(IPencuil<>).Assembly))
        .Create();
    public List<IRenderPhase<DefaultRenderContext>> RenderPhases { get; } = new();
    public PencuilOptions PencuilOptions { get; } = new() { ClearTarget = true };
    public GuiStyle GuiStyle { get; } = GuiStyles.Style;
    public List<IView> Views { get; } = new();

    public HotbarViewModel HotbarViewModel { get; } = new();

    [Singleton<Hotbar>]
    public partial IView HotbarView { get; }

    [OnActivate]
    void CollectRenderPhase(IRenderPhase<DefaultRenderContext> phase) => RenderPhases.Add(phase);

    [OnActivate]
    void CollectView(IView view) => Views.Add(view);
}

static class Program
{
    static int Main()
    {
        using HotbarApp app = new();
        return app.Run();
    }
}
