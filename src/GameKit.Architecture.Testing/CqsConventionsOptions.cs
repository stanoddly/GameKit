namespace GameKit.Architecture.Testing;

/// <summary>
/// Caller-supplied policy for <see cref="CqsConventions"/>.
/// </summary>
public sealed class CqsConventionsOptions
{
    internal bool RequiresQueryResultSuffix { get; private set; }

    /// <summary>
    /// Requires every query handler result type to be a named result object ending with <c>Result</c>.
    /// </summary>
    public CqsConventionsOptions RequireQueryResultSuffix()
    {
        RequiresQueryResultSuffix = true;
        return this;
    }
}
