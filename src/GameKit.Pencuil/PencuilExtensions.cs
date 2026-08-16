using GameKit.App;
using GameKit.Content;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;
using GameKit.Shaders;
using GameKit.Text;

namespace GameKit.Pencuil;

public static class PencuilExtensions
{
    public static GameKitAppBuilder UsePencuil<TRenderContext>(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = false)
        where TRenderContext : IRenderContext
    {
        builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
        builder.AddSingleton(GuiStyles.Style);
        PencuilOptions options = new() { Order = order, InputOrder = inputOrder, ClearTarget = clearTarget };
        ViewRegistry viewRegistry = new();

        builder.OnActivated((instance, _) =>
        {
            if (instance is IView<TRenderContext> view)
            {
                viewRegistry.Add(view);
            }
        });

        builder.OnDisposing((instance, _) =>
        {
            if (instance is IView<TRenderContext> view)
            {
                viewRegistry.Remove(view);
            }
        });

        builder.AddSingleton<PencuilState<TRenderContext>>(sp => new PencuilState<TRenderContext>(
            new Pencil(
                sp.GetRequiredService<IFontSystem>(),
                sp.GetRequiredService<IClipboardService>(),
                sp.GetRequiredService<GuiStyle>()),
            viewRegistry,
            options));

        // Factory overload required: generated constructor registration cannot bind TRenderContext yet.
        builder.AddSingleton<IRenderer<TRenderContext>, PencuilRenderer<TRenderContext>>(sp => new PencuilRenderer<TRenderContext>(
            sp.GetRequiredService<PencuilState<TRenderContext>>(),
            sp.GetRequiredService<GraphicsPipelineBuilder>(),
            sp.GetRequiredService<GpuMemorySystem>(),
            sp.GetRequiredService<ShaderLoader>(),
            sp.GetRequiredService<GpuDevice>(),
            sp.GetRequiredService<Window<TRenderContext>>()));
        builder.AddSingleton<PencilSystem<TRenderContext>>(sp => new PencilSystem<TRenderContext>(
            sp.GetRequiredService<PencuilState<TRenderContext>>(),
            sp.GetRequiredService<Window<TRenderContext>>(),
            sp.GetRequiredService<IMouseService>(),
            sp.GetRequiredService<IKeyboardService>(),
            sp.GetRequiredService<ITextInputService>()));
        return builder;
    }

    public static GameKitAppBuilder UsePencuil(this GameKitAppBuilder builder, int order = 10_000, int inputOrder = -10_000, bool clearTarget = true)
        => builder.UsePencuil<DefaultRenderContext>(order, inputOrder, clearTarget);
}
