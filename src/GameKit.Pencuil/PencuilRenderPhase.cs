using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase<TRenderContext> : IRenderPhase<TRenderContext>
{
    private readonly GuiContext _guiContext;
    private readonly GuiCanvas[] _canvases;

    public PencuilRenderPhase(GuiContext guiContext, IEnumerable<GuiCanvas> canvases)
    {
        _guiContext = guiContext;
        _canvases = canvases.ToArray();
    }

    public void Render(TRenderContext renderContext)
    {
        foreach (var canvas in _canvases)
            canvas.Build(_guiContext);

        _guiContext.Draw();
    }
}
