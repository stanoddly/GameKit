using Pixely.Architecture.Events;
using Pixely.DependencyInjection;

namespace Pixely.Architecture.Tests;

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
        ServiceRegistry<IDomainEventListener> listeners = BuildListenerRegistry(first, second);
        DomainEventDispatchHook dispatchHook = new(stream.CreateCursor(), listeners);

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
        ServiceRegistry<IDomainEventListener> listeners = BuildListenerRegistry(listener);
        DomainEventDispatchHook dispatchHook = new(stream.CreateCursor(), listeners);

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

        CommandResult result = provider.GetRequiredService<ICommandDispatcher>().Dispatch(new OuterCommand());

        Assert.That(result.IsSuccess, Is.True);
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

    [Test]
    public void Dispatch_DomainEventListenerMayDependOnDispatcherAndDispatchFollowUpCommand()
    {
        Recorder recorder = new();
        ServiceCollection services = new();
        services.AddSingleton(recorder);
        services.AddDomainEvents();
        services.AddCommandDispatching();
        services.AddDomainEventDispatchHook();
        services.AddSingleton<ICommandHandler<PublishOnlyCommand>, PublishOnlyCommandHandler>();
        services.AddSingleton<ICommandHandler<FollowUpCommand>, FollowUpCommandHandler>();
        services.AddSingleton<DispatchingListener>();
        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ICommandDispatcher>().Dispatch(new PublishOnlyCommand());

        Assert.That(recorder.Log, Is.EqualTo(new[] { "follow-up" }));
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
        return services.BuildServiceProvider();
    }

    private static ServiceRegistry<IDomainEventListener> BuildListenerRegistry(params IDomainEventListener[] listeners)
    {
        ServiceCollection services = new();
        services.AddRegistry<IDomainEventListener>();
        foreach (IDomainEventListener listener in listeners)
        {
            services.AddSingleton(listener);
        }

        ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ServiceRegistry<IDomainEventListener>>();
    }
}

internal sealed class Recorder
{
    public List<string> Log { get; } = new();
    public int HookCalls { get; set; }
}

internal sealed record OuterCommand;

internal sealed record InnerCommand;

internal sealed record PublishOnlyCommand;

internal sealed record FollowUpCommand;

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

    public CommandResult Handle(OuterCommand command)
    {
        _dispatcher.Dispatch(new InnerCommand());
        _recorder.Log.Add("outer");
        _publisher.Publish(new TestMessage(42));
        return CommandResult.Success;
    }
}

internal sealed class InnerCommandHandler : ICommandHandler<InnerCommand>
{
    private readonly Recorder _recorder;

    internal InnerCommandHandler(Recorder recorder)
    {
        _recorder = recorder;
    }

    public CommandResult Handle(InnerCommand command)
    {
        _recorder.Log.Add("inner");
        return CommandResult.Success;
    }
}

internal sealed class PublishOnlyCommandHandler : ICommandHandler<PublishOnlyCommand>
{
    private readonly IDomainEventPublisher _publisher;

    internal PublishOnlyCommandHandler(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public CommandResult Handle(PublishOnlyCommand command)
    {
        _publisher.Publish(new TestMessage(7));
        return CommandResult.Success;
    }
}

internal sealed class FollowUpCommandHandler : ICommandHandler<FollowUpCommand>
{
    private readonly Recorder _recorder;

    internal FollowUpCommandHandler(Recorder recorder)
    {
        _recorder = recorder;
    }

    public CommandResult Handle(FollowUpCommand command)
    {
        _recorder.Log.Add("follow-up");
        return CommandResult.Success;
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

internal sealed class DispatchingListener : IDomainEventListener
{
    private readonly ICommandDispatcher _dispatcher;

    internal DispatchingListener(ICommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool TryProcess(DomainMessage message)
    {
        if (message is not TestMessage)
        {
            return false;
        }

        _dispatcher.Dispatch(new FollowUpCommand());
        return true;
    }
}
