using GameKit.DependencyInjection;
using GameKit.Events;

namespace GameKit.Events.Tests;

public sealed record TestEvent(int Value);

public sealed record OtherTestEvent(int Value);

public sealed class TestEventHandler : IEventHandler<TestEvent>
{
    public int ProcessCount { get; private set; }
    public int LastValue { get; private set; }

    public void Process(TestEvent args)
    {
        ProcessCount++;
        LastValue = args.Value;
    }
}

public sealed class MultiEventHandler : IEventHandler<TestEvent>, IEventHandler<OtherTestEvent>
{
    public int TestEventCount { get; private set; }
    public int OtherEventCount { get; private set; }

    public void Process(TestEvent args)
    {
        TestEventCount++;
    }

    public void Process(OtherTestEvent args)
    {
        OtherEventCount++;
    }
}

public sealed class PlainSubscriberCandidate
{
    public int ProcessCount { get; private set; }
}

public sealed class DisposableEventHandler : IEventHandler<TestEvent>, IDisposable
{
    public int ProcessCount { get; private set; }
    public bool Disposed { get; private set; }

    public void Process(TestEvent args)
    {
        ProcessCount++;
    }

    public void Dispose()
    {
        Disposed = true;
    }
}

public abstract class OrderedEventHandler : IEventHandler<TestEvent>
{
    private readonly List<string> _calls;
    private readonly string _name;

    public OrderedEventHandler(List<string> calls, string name)
    {
        _calls = calls;
        _name = name;
    }

    public void Process(TestEvent args)
    {
        _calls.Add(_name);
    }
}

public sealed class FirstOrderedEventHandler : OrderedEventHandler
{
    public FirstOrderedEventHandler(List<string> calls)
        : base(calls, "first")
    {
    }
}

public sealed class SecondOrderedEventHandler : OrderedEventHandler
{
    public SecondOrderedEventHandler(List<string> calls)
        : base(calls, "second")
    {
    }
}

public sealed class EventBusDependencyInjectionTests
{
    [Test]
    public void AddEvents_AutoSubscribesSingletonHandler()
    {
        ServiceCollection services = new();
        services.AddEvents();
        services.AddSingleton<TestEventHandler>();

        ServiceProvider provider = services.BuildServiceProvider();
        IEventBus eventBus = provider.GetRequiredService<IEventBus>();
        TestEventHandler handler = provider.GetRequiredService<TestEventHandler>();

        eventBus.PublishEvent(new TestEvent(42));

        Assert.That(handler.ProcessCount, Is.EqualTo(1));
        Assert.That(handler.LastValue, Is.EqualTo(42));
    }

    [Test]
    public void AddEvents_CalledMultipleTimes_RegistersEventBusOnce()
    {
        ServiceCollection services = new();
        services.AddEvents();
        services.AddEvents();
        services.AddSingleton<TestEventHandler>();

        ServiceProvider provider = services.BuildServiceProvider();
        IEventBus eventBus = provider.GetRequiredService<IEventBus>();
        TestEventHandler handler = provider.GetRequiredService<TestEventHandler>();

        eventBus.PublishEvent(new TestEvent(42));

        Assert.That(provider.GetServices<IEventBus>(), Has.Count.EqualTo(1));
        Assert.That(handler.ProcessCount, Is.EqualTo(1));
    }

    [Test]
    public void AddEvents_AutoSubscribesHandlerForMultipleEventInterfaces()
    {
        ServiceCollection services = new();
        services.AddEvents();
        services.AddSingleton<MultiEventHandler>();

        ServiceProvider provider = services.BuildServiceProvider();
        IEventBus eventBus = provider.GetRequiredService<IEventBus>();
        MultiEventHandler handler = provider.GetRequiredService<MultiEventHandler>();

        eventBus.PublishEvent(new TestEvent(1));
        eventBus.PublishEvent(new OtherTestEvent(2));

        Assert.That(handler.TestEventCount, Is.EqualTo(1));
        Assert.That(handler.OtherEventCount, Is.EqualTo(1));
    }

    [Test]
    public void AddEvents_IgnoresSingletonWithoutHandlerInterfaces()
    {
        ServiceCollection services = new();
        services.AddEvents();
        services.AddSingleton<PlainSubscriberCandidate>();

        ServiceProvider provider = services.BuildServiceProvider();
        IEventBus eventBus = provider.GetRequiredService<IEventBus>();
        PlainSubscriberCandidate candidate = provider.GetRequiredService<PlainSubscriberCandidate>();

        Assert.DoesNotThrow(() => eventBus.PublishEvent(new TestEvent(1)));
        Assert.That(candidate.ProcessCount, Is.EqualTo(0));
    }

    [Test]
    public void AddEvents_DisposeUnsubscribesHandlers()
    {
        ServiceCollection services = new();
        services.AddEvents();
        services.AddSingleton<DisposableEventHandler>();

        ServiceProvider provider = services.BuildServiceProvider();
        IEventBus eventBus = provider.GetRequiredService<IEventBus>();
        DisposableEventHandler handler = provider.GetRequiredService<DisposableEventHandler>();

        eventBus.PublishEvent(new TestEvent(1));
        provider.Dispose();
        eventBus.PublishEvent(new TestEvent(2));

        Assert.That(handler.ProcessCount, Is.EqualTo(1));
        Assert.That(handler.Disposed, Is.True);
    }

    [Test]
    public void AddEvents_MultipleHandlersForSameEvent_FireInSubscriptionOrder()
    {
        List<string> calls = new();

        ServiceCollection services = new();
        services.AddEvents();
        services.AddSingleton<List<string>>(calls);
        services.AddSingleton<FirstOrderedEventHandler>();
        services.AddSingleton<SecondOrderedEventHandler>();

        ServiceProvider provider = services.BuildServiceProvider();
        IEventBus eventBus = provider.GetRequiredService<IEventBus>();

        eventBus.PublishEvent(new TestEvent(1));

        Assert.That(calls, Is.EqualTo(new[] { "first", "second" }));
    }
}
