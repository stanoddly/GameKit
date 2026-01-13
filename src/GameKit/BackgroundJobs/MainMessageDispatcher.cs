using System.Diagnostics;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Delivers messages to handlers on the main thread.
/// Implements <see cref="IUpdatable"/> and processes messages each frame within a time budget.
/// </summary>
public class MainMessageDispatcher : IUpdatable
{
    private const long FrameBudgetMs = 2;

    private readonly BackgroundWorkHub _hub;
    private readonly List<MainHandlerWrapper?> _handlers = [null];
    private readonly Stopwatch _stopwatch = new();

    internal MainMessageDispatcher(BackgroundWorkHub hub)
    {
        _hub = hub;
    }

    public void RegisterHandler<TMessage>(IMainWorkHandler<TMessage> handler) where TMessage : class
    {
        int typeId = MessageTypeId<TMessage>.Id;

        while (_handlers.Count <= typeId)
        {
            _handlers.Add(null);
        }

        _handlers[typeId] = new MainHandlerWrapper<TMessage>(handler);
    }

    public void UnregisterHandler<TMessage>() where TMessage : class
    {
        int typeId = MessageTypeId<TMessage>.Id;

        if (typeId < _handlers.Count)
        {
            _handlers[typeId] = null;
        }
    }

    public void Update()
    {
        _stopwatch.Restart();

        while (_stopwatch.ElapsedMilliseconds < FrameBudgetMs &&
               _hub.TryDequeueMainMessage(out MainMessage message))
        {
            MainHandlerWrapper? handler = message.TypeId < _handlers.Count ? _handlers[message.TypeId] : null;
            handler?.Handle(message.Payload);
        }
    }
}
