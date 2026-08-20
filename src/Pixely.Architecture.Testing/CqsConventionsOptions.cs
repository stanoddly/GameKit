namespace Pixely.Architecture.Testing;

/// <summary>
/// Caller-supplied policy for <see cref="CqsConventions"/>.
/// </summary>
public sealed class CqsConventionsOptions
{
    internal bool RequiresQdoSuffix { get; private set; }

    /// <summary>
    /// Requires every query handler result type to be a query data object ending with <c>Qdo</c>, and verifies
    /// that QDOs are behaviourless, read-only data records used only in query output graphs.
    /// </summary>
    public CqsConventionsOptions RequireQdoSuffix()
    {
        RequiresQdoSuffix = true;
        return this;
    }
}
