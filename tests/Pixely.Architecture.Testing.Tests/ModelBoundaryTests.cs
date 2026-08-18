using Pixely.Architecture.Testing.Tests.BoundaryFixtures;
using System.Text.RegularExpressions;

namespace Pixely.Architecture.Testing.Tests;

[TestFixture]
public sealed class ModelBoundaryTests
{
    private const string BoundaryNamespace = "Pixely.Architecture.Testing.Tests.BoundaryFixtures";

    private static Type[] BoundaryTypes() =>
        typeof(SpawnCommand).Assembly.GetTypes()
            .Where(type => type.Namespace == BoundaryNamespace)
            .ToArray();

    private static List<string> CheckReachability(ModelBoundaryOptions options)
    {
        Type[] allTypes = BoundaryTypes();
        Type[] publicTypes = allTypes.Where(type => type.IsPublic).ToArray();
        return ModelBoundary.ReachabilityViolations(
            publicTypes, allTypes, type => type.Namespace == BoundaryNamespace, options);
    }

    private static ModelBoundaryOptions DisallowAllOutsideSurface()
    {
        ModelBoundaryOptions options = new();
        options.DisallowOutsideSurface(new Regex(".*"), "Public types must belong to the boundary surface.");
        return options;
    }

    // --- Reachability ---

    [Test]
    public void CommandQueryAndEventTransitiveTypes_AreReachable()
    {
        List<string> violations = CheckReachability(DisallowAllOutsideSurface());

        // SpawnRequest (via command property), CountResult (via query handler return), and the event must
        // not be reported. Only the genuine leak should remain.
        Assert.That(violations, Has.None.Contains(nameof(SpawnRequest)));
        Assert.That(violations, Has.None.Contains(nameof(CountResult)));
        Assert.That(violations, Has.None.Contains(nameof(ThingSpawnedEvent)));
    }

    [Test]
    public void PublicTypeNotReachableFromSurface_IsReportedAsLeak()
    {
        List<string> violations = CheckReachability(DisallowAllOutsideSurface());

        Assert.That(violations, Has.Exactly(1).Items);
        Assert.That(violations[0], Does.Contain(nameof(LeakedInternals)));
    }

    [Test]
    public void AllowedOutsideSurfaceType_IsNotReportedAsLeak()
    {
        ModelBoundaryOptions options = new();
        options.AllowOutsideSurface(typeof(LeakedInternals), "Serializer entry point.");
        options.DisallowOutsideSurface(new Regex(".*"), "Public types must belong to the boundary surface.");

        Assert.That(CheckReachability(options), Is.Empty);
    }

    [Test]
    public void DisallowedOutsideSurfaceType_ReportsRuleReason()
    {
        ModelBoundaryOptions options = new();
        options.DisallowOutsideSurface(typeof(LeakedInternals), "This type exposes implementation details.");

        List<string> violations = CheckReachability(options);

        Assert.That(violations, Has.Exactly(1).Items);
        Assert.That(violations[0], Does.Contain(nameof(LeakedInternals))
            .And.Contain("This type exposes implementation details."));
    }

    [Test]
    public void TreatAsSurface_MakesAnOtherwiseLeakedTypeAndItsReferencesReachable()
    {
        ModelBoundaryOptions options = new();
        options.TreatAsSurface(type => type == typeof(LeakedInternals));
        options.DisallowOutsideSurface(new Regex(".*"), "Public types must belong to the boundary surface.");

        Assert.That(CheckReachability(options), Is.Empty);
    }

    // --- InternalsVisibleTo ---

    [Test]
    public void InternalsVisibleTo_SpecificDisallowRuleConsumesTargetBeforeCatchAll()
    {
        ModelBoundaryOptions options = new();
        options.AllowInternalsTo("Game.Editor", "Approved editor integration.");
        options.DisallowInternalsTo("Game.Tests", "Tests must exercise the public boundary.");
        options.DisallowInternalsTo(new Regex(".*"), "No other assembly may access internals.");

        List<string> violations = ModelBoundary.InternalsVisibleToViolations(
            ["Game.Editor", "Game.Tests"], options.InternalsRules);

        Assert.That(violations, Has.Exactly(1).Items);
        Assert.That(violations[0], Does.Contain("Game.Tests").And.Contain("Tests must exercise the public boundary."));
    }

    [Test]
    public void InternalsVisibleTo_AllowRuleConsumesTargetBeforeCatchAll()
    {
        ModelBoundaryOptions options = new();
        options.AllowInternalsTo("Game.Editor", "Approved editor integration.");
        options.DisallowInternalsTo(new Regex(".*"), "No other assembly may access internals.");

        List<string> violations = ModelBoundary.InternalsVisibleToViolations(
            ["Game.Editor"], options.InternalsRules);

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void InternalsVisibleTo_RegexMustMatchEntireAssemblyName()
    {
        ModelBoundaryOptions options = new();
        options.DisallowInternalsTo(new Regex("Game\\.Tests"), "Tests must exercise the public boundary.");

        List<string> violations = ModelBoundary.InternalsVisibleToViolations(
            ["Prefix.Game.Tests.Suffix"], options.InternalsRules);

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void InternalsVisibleTo_UnmatchedTargetIsAllowed()
    {
        ModelBoundaryOptions options = new();

        List<string> violations = ModelBoundary.InternalsVisibleToViolations(
            ["Game.Unmatched"], options.InternalsRules);

        Assert.That(violations, Is.Empty);
    }
}
