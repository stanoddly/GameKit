using GameKit.Input;

namespace GameKit.Pencuil;

public class PencuilUpdatePhase : IUpdatable
{
    private readonly Pencil _pencil;
    private readonly ViewRegistry _viewRegistry;
    private readonly ITextInputService _textInputService;
    private bool _textInputActive;

    public PencuilUpdatePhase(Pencil pencil, ViewRegistry viewRegistry, IMouseService mouseService, IKeyboardService keyboardService, ITextInputService textInputService, PencuilOptions options)
    {
        _pencil = pencil;
        _viewRegistry = viewRegistry;
        _textInputService = textInputService;

        mouseService.SubscribeMotion(options.InputOrder, (_, args) =>
        {
            pencil.CursorPosition = (Vector2Int)args.Position;
            pencil.Invalidate();
        });

        mouseService.SubscribeWindowLeave(options.InputOrder, _ =>
        {
            pencil.CursorPosition = new Vector2Int(-1, -1);
            pencil.Invalidate();
        });

        mouseService.SubscribeButtonPress(options.InputOrder, (_, args) =>
        {
            if (args.Button == MouseButton.Left)
            {
                if (pencil.IsOverInteractiveArea((Vector2Int)args.Position))
                {
                    args.Consume();
                }
            }
        });

        mouseService.SubscribeButtonRelease(options.InputOrder, (_, args) =>
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

        keyboardService.SubscribeKeyDown(options.InputOrder, (keyboard, args) =>
        {
            if (pencil.HasFocus && pencil.HandleEditingKeyDown(args.Scancode, keyboard.Shift, keyboard.Ctrl))
            {
                args.Consume();
            }
        });

        textInputService.SubscribeTextInput(options.InputOrder, args =>
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
        bool needsBuild = _pencil.NeedsUpdate | _viewRegistry.ConsumeDirty();
        ReadOnlySpan<IView> views = _viewRegistry.Views;

        foreach (IView view in views)
        {
            needsBuild |= view.ConsumeDirty();
        }

        if (needsBuild)
        {
            _pencil.FocusClaimedThisFrame = false;
            _pencil.ResetInteractionTests();

            foreach (IView view in views)
            {
                view.Build(_pencil);
            }

            if (_pencil.CursorJustReleased && _pencil.HasFocus && !_pencil.FocusClaimedThisFrame)
            {
                _pencil.Blur();
            }

            _pencil.NeedsUpdate = false;
            _pencil.InstructionsChanged = _pencil.HaveInstructionsChanged();
            _pencil.CycleInstructions();
        }

        bool hasFocus = _pencil.HasFocus;
        if (hasFocus != _textInputActive)
        {
            if (hasFocus)
            {
                _textInputService.Start();
            }
            else
            {
                _textInputService.Stop();
            }
            _textInputActive = hasFocus;
        }

        _pencil.CursorJustReleased = false;
    }
}
