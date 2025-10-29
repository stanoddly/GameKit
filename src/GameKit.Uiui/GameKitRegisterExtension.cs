using GameKit.App;

namespace GameKit.Uiui;

public static class GameKitRegisterExtension
{
    public static GameKitAppBuilder RegisterUiui(this GameKitAppBuilder builder)
    {
        builder.RegisterType<GuiResolutionProvider>();
        builder.RegisterType<WidgetService>();
        builder.RegisterFunc<GuiRenderer>(GuiRendererFactory.Create);
        return builder;
    }
}