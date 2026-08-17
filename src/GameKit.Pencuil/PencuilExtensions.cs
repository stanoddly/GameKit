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
    public static GameKitAppBuilder UsePencuil(
        this GameKitAppBuilder builder,
        int order = 10_000,
        int inputOrder = -10_000,
        bool clearTarget = false)
    {
        return UsePencuil<DefaultRenderContext>(
            builder,
            default,
            order,
            inputOrder,
            clearTarget);
    }

    public static GameKitAppBuilder UsePencuil(
        this GameKitAppBuilder builder,
        ViewScope viewScope,
        int order = 10_000,
        int inputOrder = -10_000,
        bool clearTarget = false)
    {
        return UsePencuil<DefaultRenderContext>(
            builder,
            viewScope,
            order,
            inputOrder,
            clearTarget);
    }

    public static GameKitAppBuilder UsePencuil<TRenderContext>(
        this GameKitAppBuilder builder,
        int order = 10_000,
        int inputOrder = -10_000,
        bool clearTarget = false)
        where TRenderContext : IRenderContext
    {
        return UsePencuil<TRenderContext>(
            builder,
            default,
            order,
            inputOrder,
            clearTarget);
    }

    public static GameKitAppBuilder UsePencuil<TRenderContext>(
        this GameKitAppBuilder builder,
        ViewScope viewScope,
        int order = 10_000,
        int inputOrder = -10_000,
        bool clearTarget = false)
        where TRenderContext : IRenderContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (!builder.IsRegistered<ServiceRegistry<Pencuil>>())
        {
            builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
            builder.AddSingleton(GuiStyles.Style);
            builder.AddRegistry<Pencuil>();
            builder.AddRegistry<IPencuilView>();
        }

        builder.AddSingleton<Pencuil>(provider =>
            new Pencuil(
                viewScope,
                new Pencil(
                    viewScope,
                    provider.GetRequiredService<IFontSystem>(),
                    provider.GetRequiredService<IClipboardService>(),
                    provider.GetRequiredService<GuiStyle>())));

        builder.AddSingleton<IRenderer<TRenderContext>, PencuilRenderer<TRenderContext>>(provider =>
            new PencuilRenderer<TRenderContext>(
                Pencuil.GetRequired(provider, viewScope),
                order,
                clearTarget,
                provider.GetRequiredService<GraphicsPipelineBuilder>(),
                provider.GetRequiredService<GpuMemorySystem>(),
                provider.GetRequiredService<ShaderLoader>(),
                provider.GetRequiredService<GpuDevice>(),
                provider.GetRequiredService<WindowRegistry>()));
        builder.AddSingleton<PencilSystem>(provider =>
            new PencilSystem(
                Pencuil.GetRequired(provider, viewScope),
                inputOrder,
                provider.GetRequiredService<ServiceRegistry<IPencuilView>>(),
                provider.GetRequiredService<WindowRegistry>(),
                provider.GetRequiredService<IMouseService>(),
                provider.GetRequiredService<IKeyboardService>(),
                provider.GetRequiredService<ITextInputService>()));
        return builder;
    }
}
