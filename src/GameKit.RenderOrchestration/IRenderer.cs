namespace GameKit.RenderOrchestration;

public interface IOrderable
{
    int Order => 0;
}

public interface IRenderer<TRenderContext>: IOrderable
{
    void Render(TRenderContext renderContext);
}

public class NullRenderer<TRenderContext> : IRenderer<TRenderContext>
{
    public void Render(TRenderContext renderContext)
    {
    }
}
