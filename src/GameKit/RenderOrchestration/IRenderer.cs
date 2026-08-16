namespace GameKit.RenderOrchestration;

public interface IRenderer<in TRenderContext> : IOrderable, IViewScoped
{
    void Render(TRenderContext renderContext);
}

public class NullRenderer<TRenderContext> : IRenderer<TRenderContext>
{
    public void Render(TRenderContext renderContext)
    {
    }
}
