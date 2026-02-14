using GameKit.Common;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase : IRenderPhase<DefaultRenderContext>
{
    private readonly Pencil _pencil;
    private readonly GuiCanvas[] _canvases;
    private readonly PencuilRenderer _renderer;

    public PencuilRenderPhase(Pencil pencil, IEnumerable<GuiCanvas> canvases, PencuilRenderer renderer, IMouseService mouseService)
    {
        _pencil = pencil;
        _canvases = canvases.ToArray();
        _renderer = renderer;

        mouseService.Motion += (_, args) =>
        {
            pencil.CursorPosition = new ShortVector2((short)args.Position.X, (short)args.Position.Y);
        };

        mouseService.ButtonRelease += (_, args) =>
        {
            if (args.Button == MouseButton.Left)
                pencil.CursorJustReleased = true;
        };
    }

    public void Render(DefaultRenderContext renderContext)
    {
        foreach (var canvas in _canvases)
            canvas.Build(_pencil);

        _pencil.CursorJustReleased = false;

        _renderer.Render(renderContext.CommandBuffer, _pencil);
        _renderer.Present(renderContext.CommandBuffer, renderContext.SwapchainTexture);
    }
}
