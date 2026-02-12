using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase : IRenderPhase<DefaultRenderContext>
{
    private readonly GuiContext _guiContext;
    private readonly GuiCanvas[] _canvases;
    private readonly PencuilRenderer _renderer;

    public PencuilRenderPhase(GuiContext guiContext, IEnumerable<GuiCanvas> canvases, PencuilRenderer renderer)
    {
        _guiContext = guiContext;
        _canvases = canvases.ToArray();
        _renderer = renderer;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        foreach (var canvas in _canvases)
            canvas.Build(_guiContext);

        _renderer.Render(renderContext.CommandBuffer, renderContext.SwapchainTexture, _guiContext);
    }
}
