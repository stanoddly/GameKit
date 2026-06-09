namespace GameKit.Architecture.Testing;

/// <summary>
/// The outcome of an architecture check: the list of convention violations found, if any.
/// Framework-agnostic — assert on <see cref="Violations"/> (or <see cref="IsValid"/>) from any test framework.
/// </summary>
public sealed class ArchitectureReport
{
    public ArchitectureReport(IReadOnlyList<string> violations)
    {
        Violations = violations;
    }

    public IReadOnlyList<string> Violations { get; }

    public bool IsValid => Violations.Count == 0;

    public override string ToString()
    {
        if (IsValid)
        {
            return "No architecture violations.";
        }

        return $"{Violations.Count} architecture violation(s):" + Environment.NewLine
            + string.Join(Environment.NewLine, Violations);
    }
}
