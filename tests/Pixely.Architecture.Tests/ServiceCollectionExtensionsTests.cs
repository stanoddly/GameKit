using Pixely.Architecture.Events;
using Pixely.DependencyInjection;

namespace Pixely.Architecture.Tests;

[TestFixture]
public sealed class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddDomainEvents_RegistersStreamAndAliasesToSameInstance()
    {
        ServiceCollection services = new();
        services.AddDomainEvents();
        ServiceProvider provider = services.BuildServiceProvider();

        DomainEventStream stream = provider.GetRequiredService<DomainEventStream>();
        IDomainEventPublisher publisher = provider.GetRequiredService<IDomainEventPublisher>();
        IDomainEventStream readSide = provider.GetRequiredService<IDomainEventStream>();

        Assert.That(publisher, Is.SameAs(stream));
        Assert.That(readSide, Is.SameAs(stream));
    }

    [Test]
    public void AddDomainEvents_PublishAndCursorFlowThroughResolvedServices()
    {
        ServiceCollection services = new();
        services.AddDomainEvents();
        ServiceProvider provider = services.BuildServiceProvider();

        IDomainEventPublisher publisher = provider.GetRequiredService<IDomainEventPublisher>();
        DomainEventCursor cursor = provider.GetRequiredService<DomainEventCursor>();

        publisher.Publish(new TestMessage(42));

        Assert.That(cursor.TryRead(out DomainMessage? message), Is.True);
        Assert.That(((TestMessage)message!).Value, Is.EqualTo(42));
    }

    [Test]
    public void AddDomainEvents_RegistersCursorAsTransient()
    {
        ServiceCollection services = new();
        services.AddDomainEvents();
        ServiceProvider provider = services.BuildServiceProvider();

        DomainEventCursor first = provider.GetRequiredService<DomainEventCursor>();
        DomainEventCursor second = provider.GetRequiredService<DomainEventCursor>();

        Assert.That(second, Is.Not.SameAs(first));
    }
}
