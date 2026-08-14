namespace GameKit.RenderOrchestration;

/// <summary>Associates a typed rendering graph with a window at runtime.</summary>
public interface IWindowRendering<TRenderContext>
    where TRenderContext : IRenderContext
{
    /// <summary>Attaches the graph to an existing window.</summary>
    /// <remarks>
    /// A graph supports one active attachment. Disposing the returned binding destroys an attached
    /// secondary window. Closing the window through <see cref="WindowManager"/> invalidates the binding.
    /// </remarks>
    IWindowRenderBinding Attach(WindowId windowId);
}

/// <summary>Represents the lifetime of a rendering graph's window attachment.</summary>
public interface IWindowRenderBinding : IDisposable
{
    WindowId WindowId { get; }
    bool IsActive { get; }
}
