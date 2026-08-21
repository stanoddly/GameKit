namespace Pixely.Architecture.Testing.Tests;

[TestFixture]
public sealed class CqsConventionsTests
{
    [Test]
    public void CleanModel_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(MoveCommandHandler), typeof(UnitsInRangeQueryHandler), typeof(DomainService)]);

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireBdoSuffix_WithBdoGraph_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeQuery), typeof(UnitsInRangeBdo), typeof(UnitBdo), typeof(UnitsInRangeBdoQueryHandler)],
            options => options.RequireBdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireBdoSuffix_WithGenericBdo_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeQuery), typeof(PageBdo<>), typeof(UnitBdo), typeof(PagedUnitsQueryHandler)],
            options => options.RequireBdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireBdoSuffix_WithScalarResult_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeQueryHandler)],
            options => options.RequireBdoSuffix());

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain("System.Int32").And.Contain("ending with 'Bdo'"));
    }

    [Test]
    public void RequireBdoSuffix_WithOrphanBdo_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(OrphanBdo)],
            options => options.RequireBdoSuffix());

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(OrphanBdo)).And.Contain("Model boundary graph"));
    }

    [Test]
    public void RequireBdoSuffix_WithBdoInCommandGraph_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(BdoCommandHandler), typeof(BdoCommand), typeof(UnitBdo)],
            options => options.RequireBdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireBdoSuffix_WithBdoSharedByQueryOutputAndCommandInput_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(GetSettingsQueryHandler), typeof(GetSettingsQuery), typeof(SaveSettingsCommandHandler), typeof(SaveSettingsCommand), typeof(SettingsBdo)],
            options => options.RequireBdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireBdoSuffix_WithBdoInQueryInputGraph_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(BdoInputQueryHandler), typeof(BdoInputQuery), typeof(UnitsInRangeBdo), typeof(UnitBdo)],
            options => options.RequireBdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireBdoSuffix_WithBdoInEventGraph_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(BdoEvent), typeof(UnitBdo)],
            options => options.RequireBdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireBdoSuffix_WithMutableOrphanBdo_ReportsReadOnlyAndOriginViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(MutableBdo)],
            options => options.RequireBdoSuffix());

        Assert.That(report.Violations, Has.Exactly(3).Items);
        Assert.That(report.Violations, Has.Some.Contains(nameof(MutableBdo)).And.Contains("custom methods"));
        Assert.That(report.Violations, Has.Some.Contains(nameof(MutableBdo)).And.Contains("read-only to consumers"));
        Assert.That(report.Violations, Has.Some.Contains(nameof(MutableBdo)).And.Contains("Model boundary graph"));
    }

    [Test]
    public void NonRecordCommandWithBehaviour_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(BadCommandHandler)]);

        Assert.That(report.Violations, Has.Some.Contains(nameof(BadCommand)).And.Contains("record"));
        Assert.That(report.Violations, Has.Some.Contains(nameof(BadCommand)).And.Contains("custom methods"));
    }

    [Test]
    public void PublicHandlerWithPublicConstructor_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(PublicCommandHandler)]);

        Assert.That(report.Violations, Has.Some.Contains(nameof(PublicCommandHandler)).And.Contains("must not be public"));
        Assert.That(report.Violations, Has.Some.Contains(nameof(PublicCommandHandler)).And.Contains("no public constructors"));
    }

    [Test]
    public void CommandHandlerDependingOnAnotherHandler_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(ChainingCommandHandler)]);

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(ChainingCommandHandler))
            .And.Contain(nameof(MoveCommandHandler)).And.Contain("depends on handler"));
    }

    [Test]
    public void HandlerNotEndingWithExpectedSuffix_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes([typeof(OddlyNamedExecutor)]);

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(OddlyNamedExecutor)).And.Contain("CommandHandler"));
    }

    [Test]
    public void Check_WithNoAssemblies_Throws()
    {
        Assert.That(() => CqsConventions.Check(), Throws.ArgumentException);
    }

    [Test]
    public void Check_ScansAssembliesEndToEnd()
    {
        // The test assembly contains the deliberate violations above, so a real assembly scan must surface them.
        ArchitectureReport report = CqsConventions.Check(typeof(CqsConventionsTests).Assembly);

        Assert.That(report.IsValid, Is.False);
        Assert.That(report.Violations, Has.Some.Contains(nameof(PublicCommandHandler)));
    }

    [Test]
    public void Check_WithBdoConvention_ScansAssembliesEndToEnd()
    {
        ArchitectureReport report = CqsConventions.Check(
            options => options.RequireBdoSuffix(),
            typeof(CqsConventionsTests).Assembly);

        Assert.That(report.IsValid, Is.False);
        Assert.That(report.Violations, Has.Some.Contains(nameof(OrphanBdo)).And.Contains("Model boundary graph"));
        Assert.That(report.Violations, Has.None.Contains(nameof(UnitBdo)));
    }
}
