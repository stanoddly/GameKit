using GameKit.Gpu;

namespace GameKit;

public class WindowManager : IDisposable
{
    private readonly GameKitFactory _factory;
    private readonly GpuDevice _gpuDevice;
    private readonly GameKitFrameContext _frameContext;
    private readonly PlatformInfo _platformInfo;
    private readonly Dictionary<uint, Window> _windowsById = new();
    private readonly List<Window> _windows = new();

    public Window<DefaultWindow> PrimaryWindow { get; }
    public IReadOnlyList<Window> Windows => _windows;

    public WindowManager(GameKitFactory factory, GpuDevice gpuDevice, GameKitFrameContext frameContext, AppConfig config, PlatformInfo platformInfo)
    {
        _factory = factory;
        _gpuDevice = gpuDevice;
        _frameContext = frameContext;
        _platformInfo = platformInfo;

        PrimaryWindow = factory.CreateWindow(gpuDevice, frameContext, config, platformInfo);
        Attach(PrimaryWindow);
    }

    public Window CreateWindow(WindowOptions options)
    {
        Window<DynamicWindow> window = CreateWindow<DynamicWindow>(options);
        return window;
    }

    public Window<TWindow> CreateWindow<TWindow>(WindowOptions options)
        where TWindow : class
    {
        ArgumentNullException.ThrowIfNull(options);

        Window<TWindow> window = _factory.CreateWindow<TWindow>(
            _gpuDevice,
            _frameContext,
            _platformInfo,
            options);
        Attach(window);
        return window;
    }

    public void DestroyWindow(Window window)
    {
        if (window == PrimaryWindow)
        {
            throw new InvalidOperationException("Cannot destroy the primary window.");
        }

        window.Dispose();
    }

    internal void Detach(Window window)
    {
        _windowsById.Remove(window.Id);
        _windows.Remove(window);
    }

    internal bool TryGetWindow(uint windowId, out Window window)
    {
        return _windowsById.TryGetValue(windowId, out window!);
    }

    public void Dispose()
    {
        Window[] windows = _windows.ToArray();
        for (int i = windows.Length - 1; i >= 0; i--)
        {
            windows[i].Dispose();
        }
    }

    private void Attach(Window window)
    {
        window.Attach(this);
        _windows.Add(window);
        _windowsById.Add(window.Id, window);
    }

    private sealed class DynamicWindow
    {
    }
}
