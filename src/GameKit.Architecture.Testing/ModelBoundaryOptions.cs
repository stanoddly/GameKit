using System.Text.RegularExpressions;

namespace GameKit.Architecture.Testing;

/// <summary>
/// Caller-supplied policy for <see cref="ModelBoundary"/>: ordered rules for internals access and public types
/// outside the boundary surface, plus extra types that count as part of the public CQS surface.
/// </summary>
public sealed class ModelBoundaryOptions
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    internal List<Func<Type, bool>> SurfaceSeeds { get; } = new();

    internal List<BoundaryRule<string>> InternalsRules { get; } = new();

    internal List<BoundaryRule<Type>> OutsideSurfaceRules { get; } = new();

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
    /// Allows an exact assembly name in the Model's <c>InternalsVisibleTo</c> attributes.
    /// </summary>
    public ModelBoundaryOptions AllowInternalsTo(string assemblyName, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        AddInternalsRule(candidate => string.Equals(candidate, assemblyName, StringComparison.Ordinal), true, reason);
        return this;
    }

    /// <summary>
    /// Allows assembly names fully matched by <paramref name="assemblyNamePattern"/> in the Model's
    /// <c>InternalsVisibleTo</c> attributes.
    /// </summary>
    public ModelBoundaryOptions AllowInternalsTo(Regex assemblyNamePattern, string reason)
    {
        AddInternalsRule(FullNameMatcher(assemblyNamePattern), true, reason);
        return this;
    }

    /// <summary>Disallows an exact assembly name in the Model's <c>InternalsVisibleTo</c> attributes.</summary>
    public ModelBoundaryOptions DisallowInternalsTo(string assemblyName, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        AddInternalsRule(candidate => string.Equals(candidate, assemblyName, StringComparison.Ordinal), false, reason);
        return this;
    }

    /// <summary>
    /// Disallows assembly names fully matched by <paramref name="assemblyNamePattern"/> in the Model's
    /// <c>InternalsVisibleTo</c> attributes.
    /// </summary>
    public ModelBoundaryOptions DisallowInternalsTo(Regex assemblyNamePattern, string reason)
    {
        AddInternalsRule(FullNameMatcher(assemblyNamePattern), false, reason);
        return this;
    }

    /// <summary>Allows an exact public type to remain outside the reachable boundary surface.</summary>
    public ModelBoundaryOptions AllowOutsideSurface(Type type, string reason)
    {
        ArgumentNullException.ThrowIfNull(type);
        AddOutsideSurfaceRule(candidate => candidate == type, true, reason);
        return this;
    }

    /// <summary>
    /// Allows public types whose full names are fully matched by <paramref name="typeNamePattern"/> to remain
    /// outside the reachable boundary surface.
    /// </summary>
    public ModelBoundaryOptions AllowOutsideSurface(Regex typeNamePattern, string reason)
    {
        Func<string, bool> matches = FullNameMatcher(typeNamePattern);
        AddOutsideSurfaceRule(type => matches(type.FullName ?? type.Name), true, reason);
        return this;
    }

    /// <summary>Disallows an exact public type from remaining outside the reachable boundary surface.</summary>
    public ModelBoundaryOptions DisallowOutsideSurface(Type type, string reason)
    {
        ArgumentNullException.ThrowIfNull(type);
        AddOutsideSurfaceRule(candidate => candidate == type, false, reason);
        return this;
    }

    /// <summary>
    /// Disallows public types whose full names are fully matched by <paramref name="typeNamePattern"/> from
    /// remaining outside the reachable boundary surface.
    /// </summary>
    public ModelBoundaryOptions DisallowOutsideSurface(Regex typeNamePattern, string reason)
    {
        Func<string, bool> matches = FullNameMatcher(typeNamePattern);
        AddOutsideSurfaceRule(type => matches(type.FullName ?? type.Name), false, reason);
        return this;
    }

    private void AddInternalsRule(Func<string, bool> matches, bool allows, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        InternalsRules.Add(new BoundaryRule<string>(matches, allows, reason));
    }

    private void AddOutsideSurfaceRule(Func<Type, bool> matches, bool allows, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        OutsideSurfaceRules.Add(new BoundaryRule<Type>(matches, allows, reason));
    }

    private static Func<string, bool> FullNameMatcher(Regex pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        Regex boundedPattern = new(pattern.ToString(), pattern.Options, RegexTimeout);

        return candidate =>
        {
            Match match = boundedPattern.Match(candidate);
            return match.Success && match.Index == 0 && match.Length == candidate.Length;
        };
    }
}
