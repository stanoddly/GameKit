using System.Collections.Concurrent;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Manages the concurrent queues for background jobs and results.
/// </summary>
internal class BackgroundJobQueues
{
    private readonly ConcurrentQueue<BackgroundJob>[] _priorityQueues;
    private readonly ConcurrentQueue<BackgroundJobResult> _resultQueue = new();

    public BackgroundJobQueues(int priorityLevels)
    {
        _priorityQueues = new ConcurrentQueue<BackgroundJob>[priorityLevels];
        for (int i = 0; i < priorityLevels; i++)
        {
            _priorityQueues[i] = new ConcurrentQueue<BackgroundJob>();
        }
    }

    public void EnqueueJob(BackgroundJob job, int priority)
    {
        int clampedPriority = Math.Clamp(priority, 0, _priorityQueues.Length - 1);
        _priorityQueues[clampedPriority].Enqueue(job);
    }

    public void EnqueueResult(BackgroundJobResult result)
    {
        _resultQueue.Enqueue(result);
    }

    public bool TryDequeueJob(out BackgroundJob job)
    {
        foreach (ConcurrentQueue<BackgroundJob> queue in _priorityQueues)
        {
            if (queue.TryDequeue(out job))
            {
                return true;
            }
        }

        job = default;
        return false;
    }

    public bool TryDequeueResult(out BackgroundJobResult result)
    {
        return _resultQueue.TryDequeue(out result);
    }
}
