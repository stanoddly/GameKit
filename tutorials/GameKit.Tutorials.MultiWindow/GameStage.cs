using GameKit.DependencyInjection;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

internal static class GameStage
{
    internal const string SecondaryWindowName = "inventory";

    internal static void Configure(ServiceCollection services)
    {
        services.UseWindowRendering<SecondaryRenderContext>(
            SecondaryWindowName,
            SecondaryRenderContext.Create);
        services.AddSingleton<IRenderer<DefaultRenderContext>>(PrimaryRenderer.Create);
        services.AddSingleton<IRenderer<SecondaryRenderContext>>(SecondaryWindowRenderer.Create);
        services.AddSingleton<GameController>();
    }
}
