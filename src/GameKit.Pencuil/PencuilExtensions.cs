using GameKit.App;
using GameKit.Content;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public static class PencuilExtensions
{
    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder)
    {
        builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
        builder.RegisterInstance(GuiStyles.Style);
        builder.RegisterType<Pencil>();
        builder.RegisterType<PencuilRenderer>();
        builder.RegisterType<PencuilRenderPhase>().As<IRenderPhase<DefaultRenderContext>>();
        return builder;
    }
}
