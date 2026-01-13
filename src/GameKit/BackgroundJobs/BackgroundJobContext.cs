using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

internal class BackgroundJobContext : IBackgroundJobContext
{
    private readonly BackgroundJobQueues _queues;

    public ICopyPass CopyPass { get; }

    public BackgroundJobContext(ICopyPass copyPass, BackgroundJobQueues queues)
    {
        CopyPass = copyPass;
        _queues = queues;
    }

    public void DispatchResult<TResult>(TResult result) where TResult : class
    {
        int typeId = BackgroundJobTypeId<TResult>.Id;
        _queues.EnqueueResult(new BackgroundJobResult(typeId, result));
    }

    public void DispatchJob<TTask>(TTask task, int priority = 0) where TTask : class
    {
        int typeId = BackgroundJobTypeId<TTask>.Id;
        _queues.EnqueueJob(new BackgroundJob(typeId, task), priority);
    }
}
