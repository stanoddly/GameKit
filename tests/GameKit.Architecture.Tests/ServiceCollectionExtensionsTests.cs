using GameKit.Architecture.Events;
using GameKit.DependencyInjection;

namespace GameKit.Architecture.Tests;

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
        DomainEventCursor cursor = provider.GetRequiredService<IDomainEventStream>().CreateCursor();

        publisher.Publish(new TestMessage(42));

        Assert.That(cursor.TryRead(out DomainMessage? message), Is.True);
        Assert.That(((TestMessage)message!).Value, Is.EqualTo(42));
    }
}
