using GameKit.Common;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase<TRenderContext> : IRenderPhase<TRenderContext>
    where TRenderContext : IRenderContext
{
    private readonly Pencil _pencil;
    private readonly ViewRegistry _viewRegistry;
    private readonly PencuilRenderer _renderer;
    private readonly ITextInputService _textInputService;
    private readonly bool _clearTarget;
    private bool _textInputActive;

    public int Order { get; }

    public PencuilRenderPhase(Pencil pencil, ViewRegistry viewRegistry, PencuilRenderer renderer, IMouseService mouseService, IKeyboardService keyboardService, ITextInputService textInputService, Window window, PencuilOptions options)
    {
        _pencil = pencil;
        _viewRegistry = viewRegistry;
        _renderer = renderer;
        _textInputService = textInputService;
        _clearTarget = options.ClearTarget;
        Order = options.Order;

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

        window.ResolutionChanged += args =>
        {
            pencil.UpdateViewport(args.NewSize.Width, args.NewSize.Height);
            renderer.Resize(args.NewSize);
        };
    }

    public void Render(TRenderContext renderContext)
    {
        bool needsBuild = _pencil.HasInvalidation(PencilInvalidation.RebuildInstructions) | _viewRegistry.ConsumeDirty();
        ReadOnlySpan<IView> views = _viewRegistry.Views;

        foreach (IView view in views)
        {
            needsBuild |= view.ConsumeDirty();
        }

        if (needsBuild)
        {
            _pencil.ClearInvalidation(PencilInvalidation.RebuildInstructions);
            _pencil.FocusClaimedThisFrame = false;

            foreach (IView view in views)
            {
                view.Build(_pencil);
            }

            if (_pencil.CursorJustReleased && _pencil.HasFocus && !_pencil.FocusClaimedThisFrame)
            {
                _pencil.Blur();
            }

            if (_pencil.HaveInstructionsChanged() || _pencil.HasInvalidation(PencilInvalidation.RedrawRetainedTexture))
            {
                _renderer.Render(renderContext.CommandBuffer, _pencil);
                _pencil.ClearInvalidation(PencilInvalidation.RedrawRetainedTexture);
            }

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
        _renderer.Present(renderContext.CommandBuffer, renderContext.ColorTarget, _clearTarget);
    }
}
