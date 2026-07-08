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

    public Window PrimaryWindow { get; }
    public IReadOnlyList<Window> Windows => _windows;

    public WindowManager(GameKitFactory factory, GpuDevice gpuDevice, GameKitFrameContext frameContext, AppConfig config, PlatformInfo platformInfo)
    {
        _factory = factory;
        _gpuDevice = gpuDevice;
        _frameContext = frameContext;
        _platformInfo = platformInfo;

        PrimaryWindow = factory.CreateWindow(gpuDevice, frameContext, config, platformInfo);
        _windows.Add(PrimaryWindow);
        _windowsById.Add(PrimaryWindow.Id, PrimaryWindow);
    }

    public Window CreateWindow(WindowOptions options)
    {
        AppConfig config = new(options.Size, options.Title, null, options.Fullscreen, options.Resizable, options.Transparent, options.Borderless, options.AlwaysOnTop);
        Window window = _factory.CreateWindow(_gpuDevice, _frameContext, config, _platformInfo);
        _windows.Add(window);
        _windowsById.Add(window.Id, window);
        return window;
    }

    public void DestroyWindow(Window window)
    {
        if (window == PrimaryWindow)
        {
            throw new InvalidOperationException("Cannot destroy the primary window.");
        }

        _windowsById.Remove(window.Id);
        _windows.Remove(window);
        window.Dispose();
    }

    internal bool TryGetWindow(uint windowId, out Window window)
    {
        return _windowsById.TryGetValue(windowId, out window!);
    }

    public void Dispose()
    {
        PrimaryWindow.Dispose();
    }
}
