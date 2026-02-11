using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public static class PencuilExtensions
{
    public static GameKitAppBuilder UsePencuil<TRenderContext>(this GameKitAppBuilder builder)
    {
        builder.RegisterType<NullGuiPlatform>().As<IGuiPlatform>();
        builder.RegisterInstance(GuiStyles.Style);
        builder.RegisterType<GuiContext>();
        builder.RegisterType<PencuilRenderPhase<TRenderContext>>().As<IRenderPhase<TRenderContext>>();
        return builder;
    }

    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder)
    {
        return builder.UsePencuil<DefaultRenderContext>();
    }
}
