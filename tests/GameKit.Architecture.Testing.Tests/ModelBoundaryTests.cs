using GameKit.Architecture.Testing.Tests.BoundaryFixtures;

namespace GameKit.Architecture.Testing.Tests;

[TestFixture]
public sealed class ModelBoundaryTests
{
    private const string BoundaryNamespace = "GameKit.Architecture.Testing.Tests.BoundaryFixtures";

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

    // --- Reachability ---

    [Test]
    public void CommandQueryAndEventTransitiveTypes_AreReachable()
    {
        List<string> violations = CheckReachability(new ModelBoundaryOptions());

        // SpawnRequest (via command property), CountResult (via query handler return), and the event must
        // not be reported. Only the genuine leak should remain.
        Assert.That(violations, Has.None.Contains(nameof(SpawnRequest)));
        Assert.That(violations, Has.None.Contains(nameof(CountResult)));
        Assert.That(violations, Has.None.Contains(nameof(ThingSpawnedEvent)));
    }

    [Test]
    public void PublicTypeNotReachableFromSurface_IsReportedAsLeak()
    {
        List<string> violations = CheckReachability(new ModelBoundaryOptions());

        Assert.That(violations, Has.Exactly(1).Items);
        Assert.That(violations[0], Does.Contain(nameof(LeakedInternals)));
    }

    [Test]
    public void ExcludedType_IsNotReportedAsLeak()
    {
        ModelBoundaryOptions options = new();
        options.Exclude(typeof(LeakedInternals));

        Assert.That(CheckReachability(options), Is.Empty);
    }

    [Test]
    public void TreatAsSurface_MakesAnOtherwiseLeakedTypeAndItsReferencesReachable()
    {
        ModelBoundaryOptions options = new();
        options.TreatAsSurface(type => type == typeof(LeakedInternals));

        Assert.That(CheckReachability(options), Is.Empty);
    }

    // --- InternalsVisibleTo ---

    [Test]
    public void InternalsVisibleTo_NonWhitelistedTarget_IsReported()
    {
        List<string> violations = ModelBoundary.InternalsVisibleToViolations(
            ["Game.Editor", "Game.Tests"], ["Game.Editor"]);

        Assert.That(violations, Has.Exactly(1).Items);
        Assert.That(violations[0], Does.Contain("Game.Tests"));
    }

    [Test]
    public void InternalsVisibleTo_AllWhitelisted_IsClean()
    {
        List<string> violations = ModelBoundary.InternalsVisibleToViolations(
            ["Game.Editor"], ["Game.Editor", "Game.Tests"]);

        Assert.That(violations, Is.Empty);
    }
}
