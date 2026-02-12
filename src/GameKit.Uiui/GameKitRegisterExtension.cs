using GameKit.App;
using GameKit.Content;

namespace GameKit.Uiui;

public static class GameKitRegisterExtension
{
    public static GameKitAppBuilder RegisterUiui(this GameKitAppBuilder builder)
    {
        builder.AddFileSystem(EmbeddedFileSystem.Create(typeof(GameKitRegisterExtension).Assembly));
        builder.RegisterType<GuiResolutionProvider>();
        builder.RegisterType<WidgetService>();
        builder.RegisterFunc<GuiRenderer>(GuiRendererFactory.Create);
        return builder;
    }
}