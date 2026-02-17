using System.Diagnostics.CodeAnalysis;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Defines a provider that creates and supplies a render context for a single frame.
/// </summary>
/// <typeparam name="TRenderContext">The type of the render context to provide.</typeparam>
public interface IRenderContextProvider<TRenderContext> where TRenderContext: IRenderContext
{
    /// <summary>
    /// Attempts to create and provide a render context.
    /// </summary>
    /// <param name="renderContext">When this method returns, contains the created render context, or null if creation failed.</param>
    /// <returns>True if the render context was successfully provided, false otherwise.</returns>
    public bool TryProvide([NotNullWhen(true)] out TRenderContext? renderContext);
}