using System.Collections.Concurrent;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Central hub for message passing between main and background threads.
/// </summary>
public class BackgroundWorkHub
{
    private readonly ConcurrentQueue<BackgroundMessage>[] _priorityQueues;
    private readonly ConcurrentQueue<MainMessage> _resultQueue = new();

    public BackgroundWorkHub(int priorityLevels)
    {
        _priorityQueues = new ConcurrentQueue<BackgroundMessage>[priorityLevels];
        for (int i = 0; i < priorityLevels; i++)
        {
            _priorityQueues[i] = new ConcurrentQueue<BackgroundMessage>();
        }
    }

    public void SendToBackground<TMessage>(TMessage message, int priority = 0) where TMessage : class
    {
        int typeId = MessageTypeId<TMessage>.Id;
        int clampedPriority = Math.Clamp(priority, 0, _priorityQueues.Length - 1);
        _priorityQueues[clampedPriority].Enqueue(new BackgroundMessage(typeId, message));
    }

    public void SendToMain<TMessage>(TMessage message) where TMessage : class
    {
        int typeId = MessageTypeId<TMessage>.Id;
        _resultQueue.Enqueue(new MainMessage(typeId, message));
    }

    internal bool TryDequeueBackgroundMessage(out BackgroundMessage message)
    {
        foreach (ConcurrentQueue<BackgroundMessage> queue in _priorityQueues)
        {
            if (queue.TryDequeue(out message))
            {
                return true;
            }
        }

        message = default;
        return false;
    }

    internal bool TryDequeueMainMessage(out MainMessage message)
    {
        return _resultQueue.TryDequeue(out message);
    }
}
