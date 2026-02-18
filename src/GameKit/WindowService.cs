using GameKit.Common;

namespace GameKit;

public readonly record struct ResolutionChangedEventArgs(ShortSize OldSize, ShortSize NewSize, ulong Timestamp);

public delegate void ResolutionChangedHandler(ResolutionChangedEventArgs eventArgs);

public interface IWindowService
{
    event ResolutionChangedHandler? ResolutionChanged;
}

public class WindowService : IWindowService
{
    private readonly IWindow _window;
    private ShortSize _lastSize;

    public event ResolutionChangedHandler? ResolutionChanged;

    internal WindowService(IWindow window)
    {
        _window = window;
        _lastSize = window.RenderSizeInPixels;
    }

    internal void OnWindowPixelSizeChanged(ulong timestamp)
    {
        ShortSize newSize = _window.RenderSizeInPixels;
        ShortSize oldSize = _lastSize;

        if (newSize == oldSize) return;

        _lastSize = newSize;
        ResolutionChanged?.Invoke(new ResolutionChangedEventArgs(oldSize, newSize, timestamp));
    }
}
