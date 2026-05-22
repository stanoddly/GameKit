using GameKit.Gpu;
using GameKit.Pencuil;

namespace GameKit.Tutorials.SceneSwitching;

public class SceneView : IView, IDisposable
{
    private readonly string _name;
    private readonly Color _color;
    private bool _dirty = true;

    public SceneView(string name, Color color)
    {
        _name = name;
        _color = color;
        Console.WriteLine($"SceneView created: {_name}");
    }

    public void Dispose()
    {
        Console.WriteLine($"SceneView disposed: {_name}");
    }

    public bool ConsumeDirty()
    {
        bool dirty = _dirty;
        _dirty = false;
        return dirty;
    }

    public void Build(Pencil pencil)
    {
        int panelWidth = 400;
        int panelHeight = 300;
        int x = pencil.Center.X - panelWidth / 2;
        int y = pencil.Center.Y - panelHeight / 2 + 40;

        pencil.MoveTo(x, y);
        pencil.Panel(panelWidth, panelHeight, _color);
    }
}
