namespace Pixely.Architecture.Testing;

/// <summary>
/// Caller-supplied policy for <see cref="CqsConventions"/>.
/// </summary>
public sealed class CqsConventionsOptions
{
    internal bool RequiresBdoSuffix { get; private set; }

    /// <summary>
    /// Requires every query handler result type to be a boundary data object ending with <c>Bdo</c>, and verifies
    /// that BDOs are behaviourless, read-only data records used in Model boundary graphs.
    /// </summary>
    public CqsConventionsOptions RequireBdoSuffix()
    {
        RequiresBdoSuffix = true;
        return this;
    }
}
