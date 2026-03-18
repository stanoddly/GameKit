using GameKit.App;
using GameKit.Content;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public static class PencuilExtensions
{
    public static GameKitAppBuilder UsePencuil<TRenderContext>(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = false)
        where TRenderContext : IRenderContext
    {
        builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
        builder.RegisterInstance(GuiStyles.Style);
        builder.RegisterInstance(new PencuilOptions { Order = order, InputOrder = inputOrder, ClearTarget = clearTarget });
        builder.RegisterType<Pencil>();
        builder.RegisterType<ViewRegistry>();
        builder.RegisterType<PencuilRenderer>();
        builder.RegisterType<PencuilRenderPhase<TRenderContext>>()
            .As<IRenderPhase<TRenderContext>>()
            .As<IMouseButtonReleaseHandler>()
            .As<IMouseMotionHandler>();
        return builder;
    }

    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = true)
        => builder.UsePencuil<DefaultRenderContext>(order, inputOrder, clearTarget);
}
