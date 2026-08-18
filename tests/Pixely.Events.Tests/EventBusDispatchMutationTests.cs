using Pixely.DependencyInjection;
using Pixely.Events;

namespace Pixely.Events.Tests;

public sealed class EventBusDispatchMutationTests
{
    [Test]
    public void PublishEvent_HandlerUnsubscribesItself_DoesNotThrow()
    {
        IEventBus eventBus = CreateEventBus();
        SelfUnsubscribingHandler handler = new(eventBus);

        eventBus.Subscribe<TestEvent>(handler);

        Assert.DoesNotThrow(() => eventBus.PublishEvent(new TestEvent(1)));
        eventBus.PublishEvent(new TestEvent(2));

        Assert.That(handler.ProcessCount, Is.EqualTo(1));
    }

    [Test]
    public void PublishEvent_HandlerUnsubscribesPendingHandler_PendingHandlerDoesNotRun()
    {
        IEventBus eventBus = CreateEventBus();
        PassiveHandler pendingHandler = new();
        OtherUnsubscribingHandler firstHandler = new(eventBus, pendingHandler);

        eventBus.Subscribe<TestEvent>(firstHandler);
        eventBus.Subscribe<TestEvent>(pendingHandler);

        eventBus.PublishEvent(new TestEvent(1));

        Assert.That(firstHandler.ProcessCount, Is.EqualTo(1));
        Assert.That(pendingHandler.ProcessCount, Is.EqualTo(0));
    }

    [Test]
    public void PublishEvent_HandlerSubscribesNewHandler_NewHandlerDoesNotRunUntilNextEvent()
    {
        IEventBus eventBus = CreateEventBus();
        PassiveHandler newHandler = new();
        OtherSubscribingHandler firstHandler = new(eventBus, newHandler);

        eventBus.Subscribe<TestEvent>(firstHandler);

        eventBus.PublishEvent(new TestEvent(1));
        eventBus.PublishEvent(new TestEvent(2));

        Assert.That(firstHandler.ProcessCount, Is.EqualTo(2));
        Assert.That(newHandler.ProcessCount, Is.EqualTo(1));
    }

    [Test]
    public void PublishEvents_HandlerUnsubscribesItself_DoesNotRunForRemainingEvents()
    {
        IEventBus eventBus = CreateEventBus();
        SelfUnsubscribingHandler handler = new(eventBus);

        eventBus.Subscribe<TestEvent>(handler);

        eventBus.PublishEvents(new List<TestEvent>
        {
            new(1),
            new(2)
        });

        Assert.That(handler.ProcessCount, Is.EqualTo(1));
    }

    private static IEventBus CreateEventBus()
    {
        ServiceCollection services = new();
        services.AddEvents();

        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IEventBus>();
    }

    private sealed class SelfUnsubscribingHandler : IEventHandler<TestEvent>
    {
        private readonly IEventBus _eventBus;

        public SelfUnsubscribingHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public int ProcessCount { get; private set; }

        public void Process(TestEvent args)
        {
            ProcessCount++;
            _eventBus.Unsubscribe<TestEvent>(this);
        }
    }

    private sealed class OtherUnsubscribingHandler : IEventHandler<TestEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IEventHandler<TestEvent> _handler;

        public OtherUnsubscribingHandler(IEventBus eventBus, IEventHandler<TestEvent> handler)
        {
            _eventBus = eventBus;
            _handler = handler;
        }

        public int ProcessCount { get; private set; }

        public void Process(TestEvent args)
        {
            ProcessCount++;
            _eventBus.Unsubscribe(_handler);
        }
    }

    private sealed class OtherSubscribingHandler : IEventHandler<TestEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IEventHandler<TestEvent> _handler;

        public OtherSubscribingHandler(IEventBus eventBus, IEventHandler<TestEvent> handler)
        {
            _eventBus = eventBus;
            _handler = handler;
        }

        public int ProcessCount { get; private set; }

        public void Process(TestEvent args)
        {
            ProcessCount++;
            _eventBus.Subscribe(_handler);
        }
    }

    private sealed class PassiveHandler : IEventHandler<TestEvent>
    {
        public int ProcessCount { get; private set; }

        public void Process(TestEvent args)
        {
            ProcessCount++;
        }
    }
}
