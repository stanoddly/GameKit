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

        if (!builder.IsRegistered<ServiceRegistry<PencuilState>>())
        {
            builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(PencuilExtensions).Assembly));
            builder.AddSingleton(GuiStyles.Style);
            builder.AddRegistry<PencuilState>();
        }

        PencuilOptions options = new(viewScope)
        {
            Order = order,
            InputOrder = inputOrder,
            ClearTarget = clearTarget
        };
        PencuilViewRegistry viewRegistry = new(viewScope);
        PencuilState? state = null;

        PencuilState ResolveState(ServiceProvider provider)
        {
            state ??= new PencuilState(
                viewScope,
                new Pencil(
                    viewScope,
                    provider.GetRequiredService<IFontSystem>(),
                    provider.GetRequiredService<IClipboardService>(),
                    provider.GetRequiredService<GuiStyle>()),
                viewRegistry,
                options);
            return state;
        }

        builder.OnActivated((instance, _) =>
        {
            if (instance is IPencuilView view &&
                view.ViewScope == viewScope)
            {
                viewRegistry.Add(view);
            }
        });

        builder.OnDisposing((instance, _) =>
        {
            if (instance is IPencuilView view &&
                view.ViewScope == viewScope)
            {
                viewRegistry.Remove(view);
            }
        });

        builder.AddSingleton<PencuilState>(ResolveState);
        builder.AddSingleton<IRenderer<TRenderContext>, PencuilRenderer<TRenderContext>>(provider =>
            new PencuilRenderer<TRenderContext>(
                ResolveState(provider),
                provider.GetRequiredService<GraphicsPipelineBuilder>(),
                provider.GetRequiredService<GpuMemorySystem>(),
                provider.GetRequiredService<ShaderLoader>(),
                provider.GetRequiredService<GpuDevice>(),
                provider.GetRequiredService<WindowRegistry>()));
        builder.AddSingleton<PencilSystem>(provider =>
            new PencilSystem(
                ResolveState(provider),
                provider.GetRequiredService<WindowRegistry>(),
                provider.GetRequiredService<IMouseService>(),
                provider.GetRequiredService<IKeyboardService>(),
                provider.GetRequiredService<ITextInputService>()));
        return builder;
    }
}
