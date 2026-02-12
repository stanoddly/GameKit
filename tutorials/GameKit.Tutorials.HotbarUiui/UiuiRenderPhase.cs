using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Uiui;

namespace GameKit.Tutorials.HotbarUiui;

public class UiuiGuiRendererConfig : IGuiRendererConfig
{
    public TextureFormat ColorTargetFormat { get; }
    public DepthBufferFormat DepthBufferFormat => DepthBufferFormat.Depth32;

    public UiuiGuiRendererConfig(IWindow window)
    {
        ColorTargetFormat = window.ColorTargetFormat;
    }
}

public class UiuiRenderPhase : IRenderPhase<DefaultRenderContext>
{
    private readonly GuiRenderer _guiRenderer;

    public UiuiRenderPhase(GuiRenderer guiRenderer)
    {
        _guiRenderer = guiRenderer;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        _guiRenderer.Render(renderContext.CommandBuffer);
        _guiRenderer.Present(renderContext.CommandBuffer, renderContext.SwapchainTexture);
    }
}
