using GameKit.Common;
using GameKit.Gpu;
using GameKit.Pencuil;
using GameKit.Text;

namespace GameKit.Tutorials.SceneSwitching;

public class MenuView : IView
{
    private static readonly Color BackgroundColor = new(28, 30, 34, 255);
    private static readonly Color ButtonColor = new(62, 87, 121, 255);
    private static readonly Color ButtonHoverColor = new(78, 112, 156, 255);
    private static readonly Color ActiveButtonColor = new(46, 139, 87, 255);
    private static readonly Color ActiveButtonHoverColor = new(60, 179, 113, 255);
    private static readonly Color TextColor = new(235, 238, 242, 255);

    private const int ButtonWidth = 160;
    private const int ButtonHeight = 44;
    private const int ButtonGap = 16;
    private const int TopMargin = 24;

    private readonly SceneManager _sceneManager;
    private readonly Font _font;
    private string? _activeScene;
    private bool _dirty = true;

    public MenuView(SceneManager sceneManager, IFontSystem fontSystem)
    {
        _sceneManager = sceneManager;
        _font = fontSystem.Load("fonts/GohuFont-Medium.ttf", 16);
    }

    public bool ConsumeDirty()
    {
        bool dirty = _dirty;
        _dirty = false;
        return dirty;
    }

    public void Build(Pencil pencil)
    {
        pencil.MoveTo(0, 0);
        pencil.Panel(pencil.BottomRight.X, pencil.BottomRight.Y, BackgroundColor);

        int totalWidth = ButtonWidth * 2 + ButtonGap;
        int startX = pencil.Center.X - totalWidth / 2;
        int y = TopMargin;

        if (DrawSceneButton(pencil, startX, y, "Scene A", _activeScene == "A"))
        {
            _activeScene = "A";
            _dirty = true;
            _sceneManager.LoadScene(services =>
            {
                services.AddSingleton<IView>(new SceneView("Scene A", new Color(70, 130, 180, 255)));
            });
        }

        if (DrawSceneButton(pencil, startX + ButtonWidth + ButtonGap, y, "Scene B", _activeScene == "B"))
        {
            _activeScene = "B";
            _dirty = true;
            _sceneManager.LoadScene(services =>
            {
                services.AddSingleton<IView>(new SceneView("Scene B", new Color(180, 100, 70, 255)));
            });
        }
    }

    private CursorState DrawButton(Pencil pencil, int x, int y, string text, Color color, Color hoverColor)
    {
        Rectangle area = new Rectangle(x, y, ButtonWidth, ButtonHeight);
        Color actualColor = area.Intersects(pencil.CursorPosition) ? hoverColor : color;

        pencil.MoveTo(x, y);
        CursorState state = pencil.Panel(ButtonWidth, ButtonHeight, actualColor);

        IntVector2 textSize = pencil.MeasureText(text, _font);
        pencil.MoveTo(
            x + (ButtonWidth - textSize.X) / 2,
            y + (ButtonHeight - textSize.Y) / 2);
        pencil.Text(text, _font, TextColor);

        return state;
    }

    private bool DrawSceneButton(Pencil pencil, int x, int y, string text, bool active)
    {
        Color color = active ? ActiveButtonColor : ButtonColor;
        Color hoverColor = active ? ActiveButtonHoverColor : ButtonHoverColor;
        return DrawButton(pencil, x, y, text, color, hoverColor) == CursorState.Clicked;
    }
}
