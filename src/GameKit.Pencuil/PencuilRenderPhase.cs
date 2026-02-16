using GameKit.Common;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase : IRenderPhase<DefaultRenderContext>
{
    private readonly Pencil _pencil;
    private readonly IGuiCanvas[] _canvases;
    private readonly PencuilRenderer _renderer;

    public PencuilRenderPhase(Pencil pencil, IEnumerable<IGuiCanvas> canvases, PencuilRenderer renderer, IMouseService mouseService)
    {
        _pencil = pencil;
        _canvases = canvases.ToArray();
        _renderer = renderer;

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
    }

    public void Render(DefaultRenderContext renderContext)
    {
        bool needsBuild = _pencil.NeedsUpdate;

        foreach (IGuiCanvas canvas in _canvases)
        {
            needsBuild |= canvas.ConsumeDirty();
        }

        if (needsBuild)
        {
            foreach (IGuiCanvas canvas in _canvases)
            {
                canvas.Build(_pencil);
            }

            _pencil.NeedsUpdate = false;
            _renderer.Render(renderContext.CommandBuffer, _pencil);
        }

        _pencil.CursorJustReleased = false;
        _renderer.Present(renderContext.CommandBuffer, renderContext.SwapchainTexture);
    }
}
