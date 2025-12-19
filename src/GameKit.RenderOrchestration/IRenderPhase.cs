namespace GameKit.RenderOrchestration;

/// <summary>
/// Represents an entity that can be ordered.
/// </summary>
public interface IOrderable
{
    /// <summary>
    /// The order of the entity. Lower numbers are processed first.
    /// </summary>
    int Order => 0;
}

/// <summary>
/// Represents a distinct phase in the rendering pipeline.
/// </summary>
/// <typeparam name="TRenderContext">The type of the render context required by this phase.</typeparam>
public interface IRenderPhase<in TRenderContext>: IOrderable
{
    /// <summary>
    /// Executes the rendering logic for this phase.
    /// </summary>
    /// <param name="renderContext">The render context for the current frame.</param>
    void Render(TRenderContext renderContext);
}

/// <summary>
/// A render phase that does nothing.
/// </summary>
/// <typeparam name="TRenderContext">The type of the render context.</typeparam>
public class NullRenderPhase<TRenderContext> : IRenderPhase<TRenderContext>
{
    /// <inheritdoc/>
    public void Render(TRenderContext renderContext)
    {
    }
}
