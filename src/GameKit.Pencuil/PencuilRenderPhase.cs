using GameKit.Common;
using GameKit.Input;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase : IRenderPhase<DefaultRenderContext>
{
    private readonly GuiContext _guiContext;
    private readonly GuiCanvas[] _canvases;
    private readonly PencuilRenderer _renderer;

    public PencuilRenderPhase(GuiContext guiContext, IEnumerable<GuiCanvas> canvases, PencuilRenderer renderer, IMouseService mouseService)
    {
        _guiContext = guiContext;
        _canvases = canvases.ToArray();
        _renderer = renderer;

        mouseService.Motion += (_, args) =>
        {
            guiContext.CursorPosition = new ShortVector2((short)args.Position.X, (short)args.Position.Y);
        };

        mouseService.ButtonRelease += (_, args) =>
        {
            if (args.Button == MouseButton.Left)
                guiContext.CursorJustReleased = true;
        };
    }

    public void Render(DefaultRenderContext renderContext)
    {
        foreach (var canvas in _canvases)
            canvas.Build(_guiContext);

        _guiContext.CursorJustReleased = false;

        _renderer.Render(renderContext.CommandBuffer, renderContext.SwapchainTexture, _guiContext);
    }
}
