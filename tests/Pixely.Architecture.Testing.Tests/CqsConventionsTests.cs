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
    public void RequireQueryResultSuffix_WithResultType_HasNoViolations()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeResultQueryHandler)],
            options => options.RequireQueryResultSuffix());

        Assert.That(report.IsValid, Is.True, report.ToString());
    }

    [Test]
    public void RequireQueryResultSuffix_WithScalarResult_IsReported()
    {
        ArchitectureReport report = CqsConventions.CheckTypes(
            [typeof(UnitsInRangeQueryHandler)],
            options => options.RequireQueryResultSuffix());

        Assert.That(report.Violations, Has.Exactly(1).Items);
        Assert.That(report.Violations[0], Does.Contain("System.Int32").And.Contain("ending with 'Result'"));
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
}
