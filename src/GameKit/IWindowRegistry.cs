namespace GameKit;

internal interface IWindowRegistry
{
    bool TryGetWindow(string name, out Window window);
    bool DestroyWindow(string name);
}
