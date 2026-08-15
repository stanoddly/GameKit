using GameKit.App;
using GameKit.Content;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;
using GameKit.Shaders;

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

        // Factory overload required: generated constructor registration cannot bind TRenderContext yet.
        builder.AddSingleton<IRenderer<TRenderContext>, PencuilRenderer<TRenderContext>>(sp => new PencuilRenderer<TRenderContext>(
            sp.GetRequiredService<Pencil>(),
            sp.GetRequiredService<PencuilOptions>(),
            sp.GetRequiredService<GraphicsPipelineBuilder>(),
            sp.GetRequiredService<GpuMemorySystem>(),
            sp.GetRequiredService<ShaderLoader>(),
            sp.GetRequiredService<GpuDevice>(),
            sp.GetRequiredService<Window<TRenderContext>>()));
        builder.AddSingleton<PencilSystem>(sp => new PencilSystem(
            sp.GetRequiredService<Pencil>(),
            sp.GetRequiredService<ViewRegistry>(),
            sp.GetRequiredService<Window<TRenderContext>>(),
            sp.GetRequiredService<IMouseService>(),
            sp.GetRequiredService<IKeyboardService>(),
            sp.GetRequiredService<ITextInputService>(),
            sp.GetRequiredService<PencuilOptions>()));
        return builder;
    }

    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = true)
        => builder.UsePencuil<DefaultRenderContext>(order, inputOrder, clearTarget);
}
