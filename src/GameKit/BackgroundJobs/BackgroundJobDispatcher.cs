namespace GameKit.BackgroundJobs;

/// <summary>
/// Dispatches tasks to background worker threads for processing.
/// Tasks are matched to processors by type and can be assigned a priority level.
/// </summary>
public class BackgroundJobDispatcher
{
    private readonly BackgroundJobWorkerPool _workerPool;

    public BackgroundJobDispatcher(BackgroundJobWorkerPool workerPool)
    {
        _workerPool = workerPool;
    }

    public void Dispatch<TTask>(TTask task, int priority = 0) where TTask : class
    {
        int typeId = BackgroundJobTypeId<TTask>.Id;
        _workerPool.Enqueue(new BackgroundJob(typeId, task), priority);
    }
}
