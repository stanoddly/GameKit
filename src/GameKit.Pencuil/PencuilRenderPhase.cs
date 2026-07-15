using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public class PencuilRenderPhase<TRenderContext> : IRenderPhase<TRenderContext>
    where TRenderContext : IRenderContext
{
    private readonly Pencil _pencil;
    private readonly PencuilRenderer _renderer;
    private readonly bool _clearTarget;
    private bool _retainedTextureDirty;

    public int Order { get; }

    public PencuilRenderPhase(Pencil pencil, PencuilRenderer renderer, PencuilOptions options)
    {
        _pencil = pencil;
        _renderer = renderer;
        _clearTarget = options.ClearTarget;
        Order = options.Order;
    }

    public void Render(TRenderContext renderContext)
    {
        ShortSize targetSize = renderContext.ColorTarget.Size;
        ResizeRetainedTextureIfNeeded(targetSize);

        if (_pencil.ViewportSize != targetSize)
        {
            _pencil.UpdateViewport(targetSize.Width, targetSize.Height);
        }

        if (_pencil.CompletedInstructionViewportSize != _pencil.ViewportSize)
        {
            _renderer.Clear(renderContext.CommandBuffer);
            _retainedTextureDirty = true;
            _renderer.Present(renderContext.CommandBuffer, renderContext.ColorTarget, _clearTarget);
            return;
        }

        // Retained texture dirtiness forces a redraw even when instruction content is
        // unchanged, since the retained texture itself was just resized.
        if (_pencil.InstructionsChanged || _retainedTextureDirty)
        {
            _renderer.Render(renderContext.CommandBuffer, _pencil);
            _pencil.InstructionsChanged = false;
            _retainedTextureDirty = false;
        }

        _renderer.Present(renderContext.CommandBuffer, renderContext.ColorTarget, _clearTarget);
    }

    private void ResizeRetainedTextureIfNeeded(ShortSize targetSize)
    {
        if (_renderer.RetainedTexture.Size == targetSize)
        {
            return;
        }

        _renderer.Resize(targetSize);
        _retainedTextureDirty = true;
    }
}
