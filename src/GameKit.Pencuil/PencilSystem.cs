using GameKit.Input;

namespace GameKit.Pencuil;

internal sealed class PencilSystem : IUpdatable, IViewScoped
{
    private readonly Pencil _pencil;
    private readonly PencuilViewRegistry _viewRegistry;
    private readonly Window _window;
    private readonly ITextInputService _textInputService;
    private bool _textInputActive;

    public ViewScope ViewScope { get; }

    internal PencilSystem(
        PencuilState state,
        WindowRegistry windowRegistry,
        IMouseService mouseService,
        IKeyboardService keyboardService,
        ITextInputService textInputService)
    {
        Pencil pencil = state.Pencil;
        PencuilOptions options = state.Options;
        ViewScope = state.ViewScope;
        _pencil = pencil;
        _viewRegistry = state.ViewRegistry;
        _window = windowRegistry.GetWindow(ViewScope);
        _textInputService = textInputService;

        mouseService.SubscribeMotion(ViewScope, options.InputOrder, (_, args) =>
        {
            pencil.CursorPosition = (Vector2Int)args.Position;
            pencil.Invalidate();
        });

        mouseService.SubscribeWindowLeave(ViewScope, options.InputOrder, _ =>
        {
            pencil.CursorPosition = new Vector2Int(-1, -1);
            pencil.Invalidate();
        });

        mouseService.SubscribeButtonPress(ViewScope, options.InputOrder, (_, args) =>
        {
            if (args.Button == MouseButton.Left)
            {
                if (pencil.IsOverInteractiveArea((Vector2Int)args.Position))
                {
                    args.Consume();
                }
            }
        });

        mouseService.SubscribeButtonRelease(ViewScope, options.InputOrder, (_, args) =>
        {
            if (args.Button == MouseButton.Left)
            {
                pencil.CursorJustReleased = true;
                pencil.Invalidate();

                if (pencil.IsOverInteractiveArea((Vector2Int)args.Position))
                {
                    args.Consume();
                }
            }
        });

        keyboardService.SubscribeKeyDown(ViewScope, options.InputOrder, (keyboard, args) =>
        {
            if (pencil.HasFocus && pencil.HandleEditingKeyDown(args.Scancode, keyboard.Shift, keyboard.Ctrl))
            {
                args.Consume();
            }
        });

        textInputService.SubscribeTextInput(ViewScope, options.InputOrder, args =>
        {
            if (pencil.HasFocus)
            {
                pencil.InsertText(args.Text);
                args.Consume();
            }
        });
    }

    public void Update()
    {
        ShortSize renderSize = _window.RenderSizeInPixels;
        _pencil.UpdateViewport(renderSize.Width, renderSize.Height);

        bool needsBuild = _pencil.NeedsUpdate | _viewRegistry.ConsumeDirty();
        ReadOnlySpan<IPencuilView> views = _viewRegistry.Views;

        foreach (IPencuilView view in views)
        {
            needsBuild |= view.ConsumeDirty();
        }

        if (needsBuild)
        {
            _pencil.FocusClaimedThisFrame = false;
            _pencil.ResetInteractionTests();

            foreach (IPencuilView view in views)
            {
                view.Build(_pencil);
            }

            if (_pencil.CursorJustReleased && _pencil.HasFocus && !_pencil.FocusClaimedThisFrame)
            {
                _pencil.Blur();
            }

            _pencil.NeedsUpdate = false;
            _pencil.InstructionsChanged = _pencil.HaveInstructionsChanged();
            _pencil.MarkInstructionsCompleted();
            _pencil.CycleInstructions();
        }

        bool hasFocus = _pencil.HasFocus;
        if (hasFocus != _textInputActive)
        {
            if (hasFocus)
            {
                _textInputService.Start(ViewScope);
            }
            else
            {
                _textInputService.Stop(ViewScope);
            }
            _textInputActive = hasFocus;
        }

        _pencil.CursorJustReleased = false;
    }
}
