namespace GameKit.Architecture.Testing;

internal sealed record BoundaryRule<T>(Func<T, bool> Matches, bool Allows, string Reason);

internal static class BoundaryRuleEvaluator
{
    public static List<string> Violations<T>(
        IEnumerable<T> candidates,
        IReadOnlyCollection<BoundaryRule<T>> rules,
        Func<T, string, string> describeViolation)
    {
        List<T> remaining = candidates.ToList();
        List<string> violations = new();

        foreach (BoundaryRule<T> rule in rules)
        {
            T[] matches = remaining.Where(rule.Matches).ToArray();
            if (!rule.Allows)
            {
                violations.AddRange(matches.Select(candidate => describeViolation(candidate, rule.Reason)));
            }

            foreach (T match in matches)
            {
                remaining.Remove(match);
            }
        }

        return violations;
    }
}
