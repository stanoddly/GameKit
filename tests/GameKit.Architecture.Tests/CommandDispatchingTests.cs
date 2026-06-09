using GameKit.Architecture.Events;
using GameKit.DependencyInjection;

namespace GameKit.Architecture.Tests;

[TestFixture]
public sealed class CommandDispatchingTests
{
    // --- DomainEventDispatchHook (no DI) ---

    [Test]
    public void DomainEventDispatchHook_FansEachDrainedMessageToEveryListener()
    {
        DomainEventStream stream = new();
        RecordingListener first = new();
        RecordingListener second = new();
        DomainEventDispatchHook dispatchHook = new(stream.CreateCursor(), [first, second]);

        stream.Publish(new TestMessage(1));
        stream.Publish(new TestMessage(2));
        dispatchHook.OnBatchCompleted();

        Assert.That(first.Received, Is.EqualTo(new[] { 1, 2 }));
        Assert.That(second.Received, Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void DomainEventDispatchHook_OnlyDrainsNewMessagesOnEachBatch()
    {
        DomainEventStream stream = new();
        RecordingListener listener = new();
        DomainEventDispatchHook dispatchHook = new(stream.CreateCursor(), [listener]);

        stream.Publish(new TestMessage(1));
        dispatchHook.OnBatchCompleted();
        dispatchHook.OnBatchCompleted();

        Assert.That(listener.Received, Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public void DomainEventCursor_Dispose_RemovesCursorSoTheStreamCanCompact()
    {
        DomainEventStream stream = new();
        DomainEventCursor cursor = stream.CreateCursor();

        // Fill the buffer to capacity without draining; the hook's undrained cursor pins every event.
        for (int i = 0; i < 8192; i++)
        {
            stream.Publish(new TestMessage(i));
        }

        cursor.Dispose();

        // With the cursor gone, compaction proceeds and publishing no longer hits the overflow guard.
        Assert.That(() => stream.Publish(new TestMessage(8192)), Throws.Nothing);
    }

    // --- CommandDispatcher depth gating (via DI) ---

    [Test]
    public void Dispatch_RunsHandlerAndFiresHooksOncePerTopLevelBatch()
    {
        Recorder recorder = new();
        ServiceProvider provider = BuildModel(recorder);

        bool handled = provider.GetRequiredService<ICommandDispatcher>().Dispatch(new OuterCommand());

        Assert.That(handled, Is.True);
        // Inner command runs inside the outer handler; both log, but the hook fires only once at depth 1.
        Assert.That(recorder.Log, Is.EqualTo(new[] { "inner", "outer" }));
        Assert.That(recorder.HookCalls, Is.EqualTo(1));
    }

    [Test]
    public void Dispatch_DrainsPublishedEventsToListenersAfterTheBatch()
    {
        Recorder recorder = new();
        ServiceProvider provider = BuildModel(recorder);

        provider.GetRequiredService<ICommandDispatcher>().Dispatch(new OuterCommand());

        // OuterCommandHandler publishes event 42; the hook drains it to the listener once the batch ends.
        Assert.That(provider.GetRequiredService<CapturingListener>().Received, Has.Member(42));
    }

    private static ServiceProvider BuildModel(Recorder recorder)
    {
        ServiceCollection services = new();
        services.AddSingleton(recorder);
        services.AddDomainEvents();
        services.AddCommandDispatching();
        services.AddDomainEventDispatchHook();
        services.AddSingleton<ICommandHandler<OuterCommand>, OuterCommandHandler>();
        services.AddSingleton<ICommandHandler<InnerCommand>, InnerCommandHandler>();
        services.AddSingleton<SpyHook>();
        services.AddAlias<ICommandDispatchHook, SpyHook>();
        services.AddSingleton<CapturingListener>();
        services.AddAlias<IDomainEventListener, CapturingListener>();
        return services.BuildServiceProvider();
    }
}

internal sealed class Recorder
{
    public List<string> Log { get; } = new();
    public int HookCalls { get; set; }
}

internal sealed record OuterCommand;

internal sealed record InnerCommand;

internal sealed class OuterCommandHandler : ICommandHandler<OuterCommand>
{
    private readonly ICommandDispatcher _dispatcher;
    private readonly IDomainEventPublisher _publisher;
    private readonly Recorder _recorder;

    internal OuterCommandHandler(ICommandDispatcher dispatcher, IDomainEventPublisher publisher, Recorder recorder)
    {
        _dispatcher = dispatcher;
        _publisher = publisher;
        _recorder = recorder;
    }

    public bool Handle(OuterCommand command)
    {
        _dispatcher.Dispatch(new InnerCommand());
        _recorder.Log.Add("outer");
        _publisher.Publish(new TestMessage(42));
        return true;
    }
}

internal sealed class InnerCommandHandler : ICommandHandler<InnerCommand>
{
    private readonly Recorder _recorder;

    internal InnerCommandHandler(Recorder recorder)
    {
        _recorder = recorder;
    }

    public bool Handle(InnerCommand command)
    {
        _recorder.Log.Add("inner");
        return true;
    }
}

internal sealed class SpyHook : ICommandDispatchHook
{
    private readonly Recorder _recorder;

    internal SpyHook(Recorder recorder)
    {
        _recorder = recorder;
    }

    public void OnBatchCompleted() => _recorder.HookCalls++;
}

internal sealed class RecordingListener : IDomainEventListener
{
    public List<int> Received { get; } = new();

    public bool TryProcess(DomainMessage message)
    {
        if (message is TestMessage testMessage)
        {
            Received.Add(testMessage.Value);
        }

        return true;
    }
}

internal sealed class CapturingListener : IDomainEventListener
{
    public List<int> Received { get; } = new();

    public bool TryProcess(DomainMessage message)
    {
        if (message is TestMessage testMessage)
        {
            Received.Add(testMessage.Value);
        }

        return true;
    }
}
