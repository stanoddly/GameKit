using GameKit.Common;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase<TRenderContext> : IRenderPhase<TRenderContext>, IMouseButtonReleaseHandler, IMouseMotionHandler
    where TRenderContext : IRenderContext
{
    private readonly Pencil _pencil;
    private readonly ViewRegistry _viewRegistry;
    private readonly PencuilRenderer _renderer;
    private readonly bool _clearTarget;
    private readonly int _inputOrder;

    public int Order { get; }

    int IMouseButtonReleaseHandler.Order { get => _inputOrder; }
    int IMouseMotionHandler.Order { get => _inputOrder; }

    public PencuilRenderPhase(Pencil pencil, ViewRegistry viewRegistry, PencuilRenderer renderer, IWindow window, PencuilOptions options)
    {
        _pencil = pencil;
        _viewRegistry = viewRegistry;
        _renderer = renderer;
        _clearTarget = options.ClearTarget;
        Order = options.Order;
        _inputOrder = options.InputOrder;

        window.ResolutionChanged += args =>
        {
            pencil.UpdateViewport(args.NewSize.Width, args.NewSize.Height);
            renderer.Resize(args.NewSize);
        };
    }

    public void OnButtonRelease(Mouse mouse, MouseButtonInputEvent inputEvent)
    {
        if (inputEvent.Button == MouseButton.Left)
        {
            _pencil.CursorJustReleased = true;
            _pencil.Invalidate();

            if (_pencil.IsOverInteractiveArea((IntVector2)inputEvent.Position))
            {
                inputEvent.Consumed = true;
            }
        }
    }

    public void OnMotion(Mouse mouse, MouseMotionInputEvent inputEvent)
    {
        _pencil.CursorPosition = (IntVector2)inputEvent.Position;
        _pencil.Invalidate();
    }

    public void Render(TRenderContext renderContext)
    {
        bool needsBuild = _pencil.NeedsUpdate;
        IReadOnlyList<IView> views = _viewRegistry.Views;

        foreach (IView view in views)
        {
            needsBuild |= view.ConsumeDirty();
        }

        if (needsBuild)
        {
            foreach (IView view in views)
            {
                view.Build(_pencil);
            }

            _pencil.NeedsUpdate = false;

            if (_pencil.HaveInstructionsChanged())
            {
                _renderer.Render(renderContext.CommandBuffer, _pencil);
            }

            _pencil.CycleInstructions();
        }

        _pencil.CursorJustReleased = false;
        _renderer.Present(renderContext.CommandBuffer, renderContext.ColorTarget, _clearTarget);
    }
}
