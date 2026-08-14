namespace GameKit;

internal interface IWindowRegistry
{
    void ClaimWindow(string name);
    void ReleaseWindow(string name);
    bool TryGetWindow(string name, out Window window);
}
