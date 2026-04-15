using GameKit.App;
using GameKit.Content;
using GameKit.DependencyInjection;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public static class PencuilExtensions
{
    public static GameKitAppBuilder UsePencuil<TRenderContext>(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = false)
        where TRenderContext : IRenderContext
    {
        builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
        builder.AddSingleton(GuiStyles.Style);
        builder.AddSingleton(new PencuilOptions { Order = order, InputOrder = inputOrder, ClearTarget = clearTarget });
        builder.AddSingleton<Pencil>();
        builder.AddSingleton<ViewRegistry>();
        builder.AddSingleton<PencuilRenderer>();
        builder.AddSingleton<IRenderPhase<TRenderContext>>(sp => new PencuilRenderPhase<TRenderContext>(
            sp.GetService<Pencil>(),
            sp.GetService<ViewRegistry>(),
            sp.GetService<PencuilRenderer>(),
            sp.GetService<IMouseService>(),
            sp.GetService<IWindow>(),
            sp.GetService<PencuilOptions>()));
        return builder;
    }

    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = true)
        => builder.UsePencuil<DefaultRenderContext>(order, inputOrder, clearTarget);
}
