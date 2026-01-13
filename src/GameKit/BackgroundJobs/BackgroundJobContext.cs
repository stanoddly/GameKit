using System.Collections.Concurrent;
using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

internal class BackgroundJobContext : IBackgroundJobContext
{
    private readonly ConcurrentQueue<BackgroundJobResult> _resultQueue;
    private readonly ConcurrentQueue<BackgroundJob>[] _priorityQueues;

    public ICopyPass CopyPass { get; }

    public BackgroundJobContext(
        ICopyPass copyPass,
        ConcurrentQueue<BackgroundJobResult> resultQueue,
        ConcurrentQueue<BackgroundJob>[] priorityQueues)
    {
        CopyPass = copyPass;
        _resultQueue = resultQueue;
        _priorityQueues = priorityQueues;
    }

    public void DispatchResult<TResult>(TResult result) where TResult : class
    {
        int typeId = BackgroundJobTypeId<TResult>.Id;
        _resultQueue.Enqueue(new BackgroundJobResult(typeId, result));
    }

    public void DispatchJob<TTask>(TTask task, int priority = 0) where TTask : class
    {
        int typeId = BackgroundJobTypeId<TTask>.Id;
        int clampedPriority = Math.Clamp(priority, 0, _priorityQueues.Length - 1);
        _priorityQueues[clampedPriority].Enqueue(new BackgroundJob(typeId, task));
    }
}
