using GameKit.DependencyInjection;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

internal static class MenuStage
{
    internal static void Configure(ServiceCollection services)
    {
        services.AddSingleton<IRenderer<DefaultRenderContext>>(MainWindowRenderer.Create);
        services.AddSingleton<MenuController>();
    }
}
