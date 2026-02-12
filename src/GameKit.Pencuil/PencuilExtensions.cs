using GameKit.App;
using GameKit.Content;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public static class PencuilExtensions
{
    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder)
    {
        builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
        builder.RegisterType<NullGuiPlatform>().As<IGuiPlatform>();
        builder.RegisterInstance(GuiStyles.Style);
        builder.RegisterType<GuiContext>();
        builder.RegisterType<PencuilRenderer>();
        builder.RegisterType<PencuilRenderPhase>().As<IRenderPhase<DefaultRenderContext>>();
        return builder;
    }
}
