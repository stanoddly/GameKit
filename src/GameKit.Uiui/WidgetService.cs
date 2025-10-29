using System.Numerics;
using GameKit.Input;
using GameKit;

namespace GameKit.Uiui;

public class WidgetService : IUpdatable
{
    private readonly FrameContext _frameContext;
    private readonly GuiRenderer _guiRenderer;
    private readonly IGuiRendererConfig _guiRendererConfig;
    private readonly GuiResolutionProvider _guiResolutionProvider;
    private readonly KeyboardService _keyboardService;
    private readonly GamepadService _gamepadService;
    private readonly List<Widget> _widgets = new();
    private readonly GuiContext _guiContext = new();

    public WidgetService(FrameContext frameContext, GuiRenderer guiRenderer, IGuiRendererConfig guiRendererConfig, GuiResolutionProvider guiResolutionProvider, KeyboardService keyboardService, GamepadService gamepadService)
    {
        _frameContext = frameContext;
        _guiRenderer = guiRenderer;
        _guiRendererConfig = guiRendererConfig;
        _guiResolutionProvider = guiResolutionProvider;
        _keyboardService = keyboardService;
        _gamepadService = gamepadService;
        
        _keyboardService.KeyDown += OnKeyDown;
        _keyboardService.KeyUp += OnKeyUp;
        _gamepadService.ButtonPress += OnGamepadButtonPress;
        _gamepadService.ButtonRelease += OnGamepadButtonRelease;
        _gamepadService.LeftStickMotion += OnGamepadStickMotion;
        _gamepadService.RightStickMotion += OnGamepadStickMotion;
    }

    public void AddWidget(Widget widget)
    {
        _widgets.Add(widget);
    }

    public void RemoveWidget(Widget widget)
    {
        _widgets.Remove(widget);
    }

    public void Update()
    {
        bool hasInvalidWidget = false;
    
        foreach (Widget widget in _widgets)
        {
            if (widget.Update(_frameContext))
            {
                hasInvalidWidget = true;
            }
        }
    
        if (hasInvalidWidget)
        {
            _guiRenderer.Touch();
            ArrangeAllWidgets();
            RenderAllWidgets();
        }
    }

    private void ArrangeAllWidgets()
    {
        var rectangle = _guiResolutionProvider.ResolutionInfo.WidgetBounds;
        foreach (var widget in _widgets)
        {
            widget.Arrange(_guiContext, rectangle);
        }
    }

    private void RenderAllWidgets()
    {
        foreach (var widget in _widgets)
        {
            widget.Render(_guiContext, _guiRenderer);
        }
    }

    private void OnKeyDown(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        for (int i = _widgets.Count - 1; i >= 0; i--)
        {
            if (_widgets[i].OnKeyDown(keyboard, keyEventArgs))
            {
                break;
            }
        }
    }

    private void OnKeyUp(Keyboard keyboard, KeyEventArgs keyEventArgs)
    {
        for (int i = _widgets.Count - 1; i >= 0; i--)
        {
            if (_widgets[i].OnKeyUp(keyboard, keyEventArgs))
            {
                break;
            }
        }
    }

    private void OnGamepadButtonPress(Gamepad gamepad, GamepadButton button)
    {
        for (int i = _widgets.Count - 1; i >= 0; i--)
        {
            if (_widgets[i].OnGamepadButtonPress(gamepad, button))
            {
                break;
            }
        }
    }

    private void OnGamepadButtonRelease(Gamepad gamepad, GamepadButton button)
    {
        for (int i = _widgets.Count - 1; i >= 0; i--)
        {
            if (_widgets[i].OnGamepadButtonRelease(gamepad, button))
            {
                break;
            }
        }
    }

    private void OnGamepadStickMotion(Gamepad gamepad, Vector2 motion)
    {
        for (int i = _widgets.Count - 1; i >= 0; i--)
        {
            if (_widgets[i].OnGamepadStickMotion(gamepad, motion))
            {
                break;
            }
        }
    }
}
