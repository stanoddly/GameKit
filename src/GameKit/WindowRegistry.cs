using GameKit.DependencyInjection;

namespace GameKit;

public sealed class WindowRegistry
{
    private readonly List<(uint SdlId, Window Window)> _windows = new();

    internal WindowRegistry()
    {
    }

    internal static void RegisterCallbacks(
        ServiceCollection services,
        WindowRegistry windowRegistry)
    {
        services.OnActivated((instance, _) =>
        {
            if (instance is Window window)
            {
                windowRegistry.Register(window);
            }
        });
        services.OnDisposing((instance, _) =>
        {
            if (instance is Window window)
            {
                windowRegistry.Unregister(window);
            }
        });
    }

    public Window GetWindow(ViewScope viewScope = default)
    {
        if (TryGetWindow(viewScope, out Window window))
        {
            return window;
        }

        throw new InvalidOperationException(
            $"No active window is registered for ViewScope {viewScope.Value}.");
    }

    public bool TryGetWindow(ViewScope viewScope, out Window window)
    {
        foreach ((_, Window registeredWindow) in _windows)
        {
            if (registeredWindow.ViewScope == viewScope)
            {
                window = registeredWindow;
                return true;
            }
        }

        window = null!;
        return false;
    }

    public bool TryGetWindow(out Window window)
    {
        return TryGetWindow(default(ViewScope), out window);
    }

    internal bool TryGetWindow(uint sdlWindowId, out Window window)
    {
        foreach ((uint registeredSdlId, Window registeredWindow) in _windows)
        {
            if (registeredSdlId == sdlWindowId)
            {
                window = registeredWindow;
                return true;
            }
        }

        window = null!;
        return false;
    }

    internal void Register(Window window)
    {
        if (window.ViewScope.Value < 0)
        {
            throw new InvalidOperationException(
                $"ViewScope {window.ViewScope.Value} cannot identify a window.");
        }

        foreach ((uint registeredSdlId, Window registeredWindow) in _windows)
        {
            if (ReferenceEquals(registeredWindow, window))
            {
                return;
            }

            if (registeredWindow.ViewScope == window.ViewScope)
            {
                throw new InvalidOperationException(
                    $"A window for ViewScope {window.ViewScope.Value} is already registered.");
            }

            if (registeredSdlId == window.SdlId)
            {
                throw new InvalidOperationException(
                    $"SDL window ID {window.SdlId} is already registered.");
            }
        }

        _windows.Add((window.SdlId, window));
    }

    internal void Unregister(Window window)
    {
        for (int i = 0; i < _windows.Count; i++)
        {
            if (ReferenceEquals(_windows[i].Window, window))
            {
                _windows.RemoveAt(i);
                return;
            }
        }
    }
}
