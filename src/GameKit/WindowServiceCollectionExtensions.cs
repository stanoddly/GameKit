using GameKit.DependencyInjection;
using GameKit.Input;

namespace GameKit;

public static class WindowServiceCollectionExtensions
{
    public static ServiceCollection AddWindow<TWindow>(
        this ServiceCollection services,
        WindowOptions options)
        where TWindow : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        if (services.IsRegistered<Window<TWindow>>())
        {
            throw new InvalidOperationException($"A window with identity {typeof(TWindow).Name} is already registered.");
        }

        services.AddSingleton<Window<TWindow>>(provider =>
            provider.GetRequiredService<WindowManager>().CreateWindow<TWindow>(options));
        AddInputServices<TWindow>(services);
        return services;
    }

    internal static void AddInputServices<TWindow>(ServiceCollection services)
        where TWindow : class
    {
        services.AddSingleton<KeyboardService<TWindow>>(provider =>
            provider.GetRequiredService<GameKitFactory>().CreateKeyboardService<TWindow>(
                provider.GetRequiredService<AppControl>()));
        services.AddAlias<IKeyboardService<TWindow>, KeyboardService<TWindow>>();

        services.AddSingleton<MouseService<TWindow>>(provider =>
            provider.GetRequiredService<GameKitFactory>().CreateMouseService(
                provider.GetRequiredService<Window<TWindow>>()));
        services.AddAlias<IMouseService<TWindow>, MouseService<TWindow>>();

        services.AddSingleton<TextInputService<TWindow>>(provider =>
            provider.GetRequiredService<GameKitFactory>().CreateTextInputService(
                provider.GetRequiredService<Window<TWindow>>()));
        services.AddAlias<ITextInputService<TWindow>, TextInputService<TWindow>>();

        services.AddSingleton<WindowEventSink<TWindow>>(provider => new WindowEventSink<TWindow>(
            provider.GetRequiredService<Window<TWindow>>(),
            provider.GetRequiredService<KeyboardService<TWindow>>(),
            provider.GetRequiredService<MouseService<TWindow>>(),
            provider.GetRequiredService<TextInputService<TWindow>>()));
    }
}
