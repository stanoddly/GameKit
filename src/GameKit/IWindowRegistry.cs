namespace GameKit;

internal interface IWindowRegistry
{
    WindowId PrimaryWindowId { get; }
    event Action<WindowId>? WindowDestroyed;
    bool TryGetWindow(WindowId windowId, out Window window);
    void DestroyWindow(WindowId windowId);
}
