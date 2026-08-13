using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Input;

namespace GameKit;

public static class WindowServiceCollectionExtensions
{
    public static ServiceCollection AddWindow(
        this ServiceCollection services,
        WindowOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        // TODO: Future window deactivation may allow application-owned windows to close independently.
        if (services is GameKitAppBuilder &&
            !options.StopGameOnClose)
        {
            throw new InvalidOperationException(
                "A window owned by the application service container cannot be closed independently. Set StopGameOnClose to true.");
        }

        if (services.IsRegistered<Window>())
        {
            throw new InvalidOperationException(
                "A window is already registered in this service hierarchy.");
        }

        AddWindowServices(services, options);
        return services;
    }

    private static void AddWindowServices(ServiceCollection services, WindowOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<ActivationWindow, GameKitFactory>();
        services.AddSingleton<Window>(Window.Create);

        services.AddSingleton<KeyboardService, GameKitFactory>();
        services.AddAlias<IKeyboardService, KeyboardService>();
        services.AddSingleton<MouseService, GameKitFactory>();
        services.AddAlias<IMouseService, MouseService>();
        services.AddSingleton<TextInputService, GameKitFactory>();
        services.AddAlias<ITextInputService, TextInputService>();

        services.AddSingleton<WindowEventService>(WindowEventService.Create);
    }
}
