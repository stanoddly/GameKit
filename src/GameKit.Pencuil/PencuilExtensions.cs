using GameKit.App;
using GameKit.Content;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public static class PencuilExtensions
{
    public static GameKitAppBuilder UsePencuil<TRenderContext>(this GameKitAppBuilder builder)
        where TRenderContext : DefaultRenderContext, IColorTarget
    {
        builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
        builder.RegisterInstance(GuiStyles.Style);
        builder.RegisterType<Pencil>();
        builder.RegisterType<PencuilRenderer>();
        builder.RegisterType<PencuilRenderPhase<TRenderContext>>().As<IRenderPhase<TRenderContext>>();
        return builder;
    }

    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder)
        => builder.UsePencuil<DefaultRenderContext>();
}
