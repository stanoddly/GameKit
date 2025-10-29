namespace GameKit.RenderOrchestration;

public interface IRenderer<TRenderContext>
{
    int Order { get; }
    void Render(TRenderContext renderContext);
}