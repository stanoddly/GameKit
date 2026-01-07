using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

public abstract class BackgroundTaskProcessor<TTask, TResult>
    where TTask : class
    where TResult : class
{
    public abstract TResult? Process(TTask task, ICopyPass copyPass);
}
