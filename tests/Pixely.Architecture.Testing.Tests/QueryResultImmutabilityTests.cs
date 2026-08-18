using Pixely.Architecture.Testing.Tests.QueryResultFixtures;

namespace Pixely.Architecture.Testing.Tests;

[TestFixture]
public sealed class QueryResultImmutabilityTests
{
    [Test]
    public void ReadonlyResults_IncludingInternalSetterAndReadOnlyCollections_AreClean()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(GoodResultQueryHandler), typeof(InternalSetterQueryHandler)]);

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void PublicSetterResult_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(PublicSetterQueryHandler)]);

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(PublicSetterResult)).And.Contain("public setter"));
    }

    [Test]
    public void MutableCollectionResult_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(ListQueryHandler)]);

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(ListResult)));
    }

    [Test]
    public void ArrayResult_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(ArrayQueryHandler)]);

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(ArrayResult)).And.Contain("array"));
    }

    [Test]
    public void RecursivelyMutableResult_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(NestedQueryHandler)]);

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(MutableInner)).And.Contain("public setter"));
    }
}
