using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Base class for processing background jobs. Implement this to define how a task type is processed.
/// Each worker thread creates its own instance via the factory registered with <see cref="BackgroundJobWorkerPool"/>.
/// </summary>
/// <typeparam name="TTask">The task type this processor handles.</typeparam>
/// <typeparam name="TResult">The result type produced, or use a marker type if no result is needed.</typeparam>
public abstract class BackgroundTaskProcessor<TTask, TResult>
    where TTask : class
    where TResult : class
{
    public abstract TResult? Process(TTask task, ICopyPass copyPass);
}
