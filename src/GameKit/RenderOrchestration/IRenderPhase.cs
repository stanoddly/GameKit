namespace GameKit.RenderOrchestration;

public interface IRenderPhase<in TRenderContext> : IOrderable
{
    void Render(TRenderContext renderContext);
}

public class NullRenderPhase<TRenderContext> : IRenderPhase<TRenderContext>
{
    public void Render(TRenderContext renderContext)
    {
    }
}
