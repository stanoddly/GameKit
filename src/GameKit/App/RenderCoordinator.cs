namespace GameKit.App;

/// <summary>
/// Coordinates rendering for exactly one logical window. The required <see cref="GameKit.Window"/>
/// makes that association part of the type and prevents a render coordinator from being constructed
/// by a windowless service container.
/// </summary>
public abstract class RenderCoordinator
{
    protected RenderCoordinator(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        Window = window;
    }

    /// <summary>The logical window whose rendering is coordinated by this instance.</summary>
    protected Window Window { get; }

    public abstract void Execute();
}
