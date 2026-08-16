namespace GameKit.RenderOrchestration;

public interface IViewRenderer : IViewScoped, IOrderable
{
    void Render(ViewRenderContext renderContext);
}

public sealed class NullViewRenderer : IViewRenderer
{
    public ViewScope ViewScope { get; }

    public NullViewRenderer(ViewScope viewScope)
    {
        ViewScope = viewScope;
    }

    public void Render(ViewRenderContext renderContext)
    {
    }
}
