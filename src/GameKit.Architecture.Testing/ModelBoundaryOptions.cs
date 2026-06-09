namespace GameKit.Architecture.Testing;

/// <summary>
/// Caller-supplied policy for <see cref="ModelBoundary"/>: which assemblies may see the Model's internals,
/// which extra types count as part of the public CQS surface, and which public types are exempt from the
/// reachability check.
/// </summary>
public sealed class ModelBoundaryOptions
{
    internal List<Func<Type, bool>> SurfaceSeeds { get; } = new();

    internal List<string> AllowedInternalsTargets { get; } = new();

    internal HashSet<Type> ExcludedTypes { get; } = new();

    /// <summary>
    /// Treats any type matching <paramref name="predicate"/> as an intentional part of the CQS surface
    /// (e.g. DI modules, marker-interface roots) so it and what it references are considered reachable.
    /// Commands, queries, and <see cref="Events.DomainMessage"/> events are surface by default.
    /// </summary>
    public ModelBoundaryOptions TreatAsSurface(Func<Type, bool> predicate)
    {
        SurfaceSeeds.Add(predicate);
        return this;
    }

    /// <summary>
    /// Whitelists assembly names allowed in the Model's <c>InternalsVisibleTo</c> attributes. Any target not
    /// listed is reported. Names must match the attribute value exactly.
    /// </summary>
    public ModelBoundaryOptions AllowInternalsTo(params string[] assemblyNames)
    {
        AllowedInternalsTargets.AddRange(assemblyNames);
        return this;
    }

    /// <summary>Exempts specific public types from the reachability check.</summary>
    public ModelBoundaryOptions Exclude(params Type[] types)
    {
        foreach (Type type in types)
        {
            ExcludedTypes.Add(type);
        }

        return this;
    }
}
