using GameKit.Gpu;

namespace GameKit;

public class WindowManager : IWindowRegistry, IDisposable
{
    private readonly GameKitFactory _factory;
    private readonly GpuDevice _gpuDevice;
    private readonly GameKitFrameContext _frameContext;
    private readonly PlatformInfo _platformInfo;
    private readonly Dictionary<WindowId, Window> _windowsById = new();
    private readonly Dictionary<uint, WindowId> _windowIdsBySdlId = new();
    private readonly List<Window> _windows = new();
    private ulong _lastWindowId;
    private bool _disposed;

    public Window PrimaryWindow { get; }
    public WindowId PrimaryWindowId { get; }
    public IReadOnlyList<Window> Windows => _windows;

    event Action<WindowId>? IWindowRegistry.WindowDestroyed
    {
        add => WindowDestroyed += value;
        remove => WindowDestroyed -= value;
    }

    private event Action<WindowId>? WindowDestroyed;

    public WindowManager(GameKitFactory factory, GpuDevice gpuDevice, GameKitFrameContext frameContext, AppConfig config, PlatformInfo platformInfo)
    {
        _factory = factory;
        _gpuDevice = gpuDevice;
        _frameContext = frameContext;
        _platformInfo = platformInfo;

        PrimaryWindow = factory.CreateWindow(gpuDevice, frameContext, config, platformInfo);
        PrimaryWindowId = RegisterWindow(PrimaryWindow);
    }

    /// <summary>Creates a secondary window and returns its opaque application identifier.</summary>
    public WindowId CreateWindow(WindowOptions options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        AppConfig config = new(options.Size, options.Title, null, options.Fullscreen, options.Resizable, options.Transparent, options.Borderless, options.AlwaysOnTop);
        Window window = _factory.CreateWindow(_gpuDevice, _frameContext, config, _platformInfo);
        return RegisterWindow(window);
    }

    public void DestroyWindow(Window window)
    {
        if (window == PrimaryWindow)
        {
            throw new InvalidOperationException("Cannot destroy the primary window.");
        }

        if (!_windowIdsBySdlId.TryGetValue(window.Id, out WindowId windowId) ||
            !_windowsById.TryGetValue(windowId, out Window? registeredWindow) ||
            !ReferenceEquals(window, registeredWindow))
        {
            return;
        }

        DestroyWindow(windowId);
    }

    /// <summary>Destroys a secondary window if its identifier is still active.</summary>
    public bool DestroyWindow(WindowId windowId)
    {
        if (!_windowsById.TryGetValue(windowId, out Window? window))
        {
            return false;
        }

        if (window == PrimaryWindow)
        {
            throw new InvalidOperationException("Cannot destroy the primary window.");
        }

        _windowsById.Remove(windowId);
        _windowIdsBySdlId.Remove(window.Id);
        _windows.Remove(window);
        WindowDestroyed?.Invoke(windowId);
        window.Dispose();
        return true;
    }

    internal bool TryGetWindow(uint windowId, out Window window)
    {
        if (_windowIdsBySdlId.TryGetValue(windowId, out WindowId id))
        {
            return _windowsById.TryGetValue(id, out window!);
        }

        window = null!;
        return false;
    }

    bool IWindowRegistry.TryGetWindow(WindowId windowId, out Window window)
    {
        return _windowsById.TryGetValue(windowId, out window!);
    }

    void IWindowRegistry.DestroyWindow(WindowId windowId)
    {
        DestroyWindow(windowId);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        for (int i = _windows.Count - 1; i >= 1; i--)
        {
            Window window = _windows[i];
            DestroyWindow(_windowIdsBySdlId[window.Id]);
        }

        _windowsById.Remove(PrimaryWindowId);
        _windowIdsBySdlId.Remove(PrimaryWindow.Id);
        _windows.Remove(PrimaryWindow);
        WindowDestroyed?.Invoke(PrimaryWindowId);
        PrimaryWindow.Dispose();
    }

    private WindowId RegisterWindow(Window window)
    {
        WindowId windowId = new(++_lastWindowId);
        _windows.Add(window);
        _windowsById.Add(windowId, window);
        _windowIdsBySdlId.Add(window.Id, windowId);
        return windowId;
    }
}
