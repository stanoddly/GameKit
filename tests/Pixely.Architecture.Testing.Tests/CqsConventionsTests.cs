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
    public void RequireQdoSuffix_WithQdoGraph_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeQuery), typeof(UnitsInRangeQdo), typeof(UnitQdo), typeof(UnitsInRangeQdoQueryHandler)],
            options => options.RequireQdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireQdoSuffix_WithGenericQdo_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeQuery), typeof(PageQdo<>), typeof(UnitQdo), typeof(PagedUnitsQueryHandler)],
            options => options.RequireQdoSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireQdoSuffix_WithScalarResult_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeQueryHandler)],
            options => options.RequireQdoSuffix());

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain("System.Int32").And.Contain("ending with 'Qdo'"));
    }

    [Test]
    public void RequireQdoSuffix_WithOrphanQdo_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(OrphanQdo)],
            options => options.RequireQdoSuffix());

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(OrphanQdo)).And.Contain("query output graph"));
    }

    [TestCase(typeof(QdoCommandHandler), typeof(QdoCommand), "command graph")]
    [TestCase(typeof(QdoInputQueryHandler), typeof(QdoInputQuery), "query input graph")]
    public void RequireQdoSuffix_WithQdoInInputGraph_IsReported(Type handlerType, Type inputType, string expectedRole)
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [handlerType, inputType, typeof(UnitsInRangeQuery), typeof(UnitsInRangeQdo), typeof(UnitQdo), typeof(UnitsInRangeQdoQueryHandler)],
            options => options.RequireQdoSuffix());

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(UnitQdo)).And.Contain(expectedRole));
    }

    [Test]
    public void RequireQdoSuffix_WithQdoInEventGraph_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(QdoEvent), typeof(UnitQdo), typeof(UnitsInRangeQdo), typeof(UnitsInRangeQdoQueryHandler)],
            options => options.RequireQdoSuffix());

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain(nameof(UnitQdo)).And.Contain("event graph"));
    }

    [Test]
    public void RequireQdoSuffix_WithMutableOrphanQdo_ReportsReadOnlyAndOriginViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(MutableQdo)],
            options => options.RequireQdoSuffix());

        Assert.That(report.Violations, Has.Exactly(3).Items);
        Assert.That(report.Violations, Has.Some.Contains(nameof(MutableQdo)).And.Contains("custom methods"));
        Assert.That(report.Violations, Has.Some.Contains(nameof(MutableQdo)).And.Contains("read-only to consumers"));
        Assert.That(report.Violations, Has.Some.Contains(nameof(MutableQdo)).And.Contains("query output graph"));
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
    public void Check_WithQdoConvention_ScansAssembliesEndToEnd()
    {
        ArchitectureReport report = CqsConventions.Check(
            options => options.RequireQdoSuffix(),
            typeof(CqsConventionsTests).Assembly);

        Assert.That(report.IsValid, Is.False);
        Assert.That(report.Violations, Has.Some.Contains(nameof(OrphanQdo)).And.Contains("query output graph"));
        Assert.That(report.Violations, Has.Some.Contains(nameof(UnitQdo)).And.Contains("command graph"));
    }
}
