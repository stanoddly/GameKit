using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

internal sealed class PencilSystem<TRenderContext> : IUpdatable
    where TRenderContext : IRenderContext
{
    private readonly Pencil _pencil;
    private readonly ViewRegistry _viewRegistry;
    private readonly Window<TRenderContext> _window;
    private readonly ITextInputService _textInputService;
    private bool _textInputActive;

    internal PencilSystem(
        PencuilState<TRenderContext> state,
        Window<TRenderContext> window,
        IMouseService mouseService,
        IKeyboardService keyboardService,
        ITextInputService textInputService)
    {
        Pencil pencil = state.Pencil;
        PencuilOptions options = state.Options;
        _pencil = pencil;
        _viewRegistry = state.ViewRegistry;
        _window = window;
        _textInputService = textInputService;

        mouseService.SubscribeMotion(options.InputOrder, (_, args) =>
        {
            if (!ReferenceEquals(args.Window, window))
            {
                return;
            }

            pencil.CursorPosition = (Vector2Int)args.Position;
            pencil.Invalidate();
        });

        mouseService.SubscribeWindowLeave(options.InputOrder, args =>
        {
            if (!ReferenceEquals(args.Window, window))
            {
                return;
            }

            pencil.CursorPosition = new Vector2Int(-1, -1);
            pencil.Invalidate();
        });

        mouseService.SubscribeButtonPress(options.InputOrder, (_, args) =>
        {
            if (!ReferenceEquals(args.Window, window))
            {
                return;
            }

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
            if (!ReferenceEquals(args.Window, window))
            {
                return;
            }

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
            if (!ReferenceEquals(args.Window, window))
            {
                return;
            }

            if (pencil.HasFocus && pencil.HandleEditingKeyDown(args.Scancode, keyboard.Shift, keyboard.Ctrl))
            {
                args.Consume();
            }
        });

        textInputService.SubscribeTextInput(options.InputOrder, args =>
        {
            if (ReferenceEquals(args.Window, _window) && pencil.HasFocus)
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
            _pencil.MarkInstructionsCompleted();
            _pencil.CycleInstructions();
        }

        bool hasFocus = _pencil.HasFocus;
        if (hasFocus != _textInputActive)
        {
            if (hasFocus)
            {
                _textInputService.Start(_window);
            }
            else
            {
                _textInputService.Stop(_window);
            }
            _textInputActive = hasFocus;
        }

        _pencil.CursorJustReleased = false;
    }
}
