using GameKit.DependencyInjection;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

internal static class GameStage
{
    internal static void Configure(ServiceCollection services)
    {
        services.UseWindowRendering<SecondaryRenderContext>(
            new WindowConfig(
                Size: new Size<uint>(480, 360),
                Title: "Secondary window - right-click to return"),
            SecondaryRenderContext.Create);
        services.AddSingleton<IRenderer<DefaultRenderContext>>(PrimaryRenderer.Create);
        services.AddSingleton<IRenderer<SecondaryRenderContext>>(SecondaryWindowRenderer.Create);
        services.AddSingleton<GameController>();
    }
}
