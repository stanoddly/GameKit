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
        builder.AddSingleton<PencuilRenderer>();

        ViewRegistry viewRegistry = new();
        builder.AddSingleton(viewRegistry);

        builder.OnActivated((instance, _) =>
        {
            if (instance is IView view)
            {
                viewRegistry.Add(view);
            }
        });

        builder.OnDisposing((instance, _) =>
        {
            if (instance is IView view)
            {
                viewRegistry.Remove(view);
            }
        });

        builder.AddSingleton<IRenderPhase<TRenderContext>>(sp => new PencuilRenderPhase<TRenderContext>(
            sp.GetRequiredService<Pencil>(),
            viewRegistry,
            sp.GetRequiredService<PencuilRenderer>(),
            sp.GetRequiredService<IMouseService>(),
            sp.GetRequiredService<IKeyboardService>(),
            sp.GetRequiredService<ITextInputService>(),
            sp.GetRequiredService<IWindow>(),
            sp.GetRequiredService<PencuilOptions>()));
        return builder;
    }

    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = true)
        => builder.UsePencuil<DefaultRenderContext>(order, inputOrder, clearTarget);
}
