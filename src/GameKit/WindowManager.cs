using GameKit.Gpu;

namespace GameKit;

public class WindowManager : IWindowRegistry, IDisposable
{
    public const string PrimaryWindowName = "main";

    private readonly GameKitFactory _factory;
    private readonly GpuDevice _gpuDevice;
    private readonly GameKitFrameContext _frameContext;
    private readonly PlatformInfo _platformInfo;
    private readonly Dictionary<string, Window> _windowsByName = new(StringComparer.Ordinal);
    private readonly Dictionary<uint, string> _windowNamesBySdlId = new();
    private readonly HashSet<string> _claimedWindowNames = new(StringComparer.Ordinal);
    private readonly List<Window> _windows = new();
    private bool _disposed;

    public Window PrimaryWindow { get; }
    public IReadOnlyList<Window> Windows => _windows;

    public WindowManager(
        GameKitFactory factory,
        GpuDevice gpuDevice,
        GameKitFrameContext frameContext,
        WindowConfig config,
        PlatformInfo platformInfo)
    {
        _factory = factory;
        _gpuDevice = gpuDevice;
        _frameContext = frameContext;
        _platformInfo = platformInfo;

        PrimaryWindow = factory.CreateWindow(gpuDevice, frameContext, config, platformInfo);
        RegisterWindow(PrimaryWindowName, PrimaryWindow);
    }

    /// <summary>Creates the secondary window claimed by a render coordinator.</summary>
    public Window CreateWindow(string name, WindowConfig config)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(config);

        if (!_claimedWindowNames.Contains(name))
        {
            throw new InvalidOperationException(
                $"Window '{name}' has not been claimed by a render coordinator.");
        }

        if (_windowsByName.ContainsKey(name))
        {
            throw new InvalidOperationException($"Window '{name}' is already open.");
        }

        Window window = _factory.CreateWindow(_gpuDevice, _frameContext, config, _platformInfo);
        RegisterWindow(name, window);
        return window;
    }

    public bool IsWindowOpen(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _windowsByName.ContainsKey(name);
    }

    public void DestroyWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (window == PrimaryWindow)
        {
            throw new InvalidOperationException("Cannot destroy the primary window.");
        }

        if (!_windowNamesBySdlId.TryGetValue(window.Id, out string? name) ||
            !_windowsByName.TryGetValue(name, out Window? registeredWindow) ||
            !ReferenceEquals(window, registeredWindow))
        {
            return;
        }

        DestroyWindow(name);
    }

    /// <summary>Destroys a secondary window if it is open.</summary>
    public bool DestroyWindow(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name == PrimaryWindowName)
        {
            throw new InvalidOperationException("Cannot destroy the primary window.");
        }

        if (!_windowsByName.Remove(name, out Window? window))
        {
            return false;
        }

        _windowNamesBySdlId.Remove(window.Id);
        _windows.Remove(window);
        window.Dispose();
        return true;
    }

    internal bool TryGetWindow(uint windowId, out Window window)
    {
        if (_windowNamesBySdlId.TryGetValue(windowId, out string? name))
        {
            return _windowsByName.TryGetValue(name, out window!);
        }

        window = null!;
        return false;
    }

    void IWindowRegistry.ClaimWindow(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_claimedWindowNames.Add(name))
        {
            throw new InvalidOperationException(
                $"Window '{name}' is already claimed by another render coordinator.");
        }
    }

    void IWindowRegistry.ReleaseWindow(string name)
    {
        if (!_claimedWindowNames.Remove(name))
        {
            return;
        }

        if (name != PrimaryWindowName)
        {
            DestroyWindow(name);
        }
    }

    bool IWindowRegistry.TryGetWindow(string name, out Window window)
    {
        return _windowsByName.TryGetValue(name, out window!);
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
            DestroyWindow(_windowNamesBySdlId[window.Id]);
        }

        _windowsByName.Remove(PrimaryWindowName);
        _windowNamesBySdlId.Remove(PrimaryWindow.Id);
        _windows.Remove(PrimaryWindow);
        _claimedWindowNames.Clear();
        PrimaryWindow.Dispose();
    }

    private void RegisterWindow(string name, Window window)
    {
        _windows.Add(window);
        _windowsByName.Add(name, window);
        _windowNamesBySdlId.Add(window.Id, name);
    }
}
