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
    private readonly bool _clearTarget;

    public int Order { get; }

    public PencuilRenderPhase(Pencil pencil, ViewRegistry viewRegistry, PencuilRenderer renderer, IMouseService mouseService, IWindow window, PencuilOptions options)
    {
        _pencil = pencil;
        _viewRegistry = viewRegistry;
        _renderer = renderer;
        _clearTarget = options.ClearTarget;
        Order = options.Order;

        mouseService.Motion += (_, args) =>
        {
            pencil.CursorPosition = (IntVector2)args.Position;
            pencil.Invalidate();
        };

        mouseService.ButtonRelease += (_, args) =>
        {
            if (args.Button == MouseButton.Left)
            {
                pencil.CursorJustReleased = true;
                pencil.Invalidate();
            }
        };

        window.ResolutionChanged += args =>
        {
            pencil.UpdateViewport(args.NewSize.Width, args.NewSize.Height);
            renderer.Resize(args.NewSize);
        };
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
