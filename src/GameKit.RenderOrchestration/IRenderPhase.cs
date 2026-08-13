namespace GameKit.RenderOrchestration;

/// <summary>
/// Represents a distinct phase in the rendering pipeline. This could include stages like
/// culling, shadow map generation, deferred shading (e.g., lighting, ambient occlusion),
/// post-processing, and UI rendering.
/// </summary>
/// <remarks>
/// Register a phase in the same service container as its default render coordinator or in a descendant
/// container. Phase discovery follows service activation: a phase already activated in an ancestor
/// is not retroactively attached to a render coordinator created by a child container.
/// </remarks>
/// <typeparam name="TRenderContext">The type of the render context required by this phase.</typeparam>
public interface IRenderPhase<in TRenderContext> : IOrderable
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
