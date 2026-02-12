using GameKit.App;
using GameKit.RenderOrchestration;
using GameKit.Uiui;

namespace GameKit.Tutorials.HotbarUiui;

static class Program
{
    static int Main(string[] args)
    {
        var builder = new GameKitAppBuilder()
            .UseDefaultRenderManager()
            .RegisterUiui();

        builder.RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Hotbar (Uiui)" });
        builder.RegisterType<UiuiGuiRendererConfig>().As<IGuiRendererConfig>();
        builder.RegisterType<UiuiRenderPhase>().As<IRenderPhase<DefaultRenderContext>>();

        builder.OnStart((WidgetService widgetService, HotbarWidget hotbar) =>
        {
            var root = new AnchorLayout(HorizontalAnchor.Center, VerticalAnchor.Bottom)
            {
                new PaddingWidget(new Padding(0, 16, 0, 0))
                {
                    hotbar
                }
            };
            widgetService.AddWidget(root);
        });

        builder.RegisterType<HotbarWidget>();

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
