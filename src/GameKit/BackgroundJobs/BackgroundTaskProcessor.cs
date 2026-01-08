using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Factory interface for creating background task processors.
/// Implement this to define how processors are created, with dependencies injected via the constructor.
/// </summary>
/// <typeparam name="TTask">The task type the processor handles.</typeparam>
/// <typeparam name="TResult">The result type the processor produces.</typeparam>
public interface IProcessorFactory<TTask, TResult>
    where TTask : class
    where TResult : class
{
    BackgroundTaskProcessor<TTask, TResult> Create();
}

/// <summary>
/// Base class for processing background jobs. Implement this to define how a task type is processed.
/// Each worker thread creates its own instance via the factory registered with <see cref="BackgroundJobWorkerPool"/>.
/// </summary>
/// <typeparam name="TTask">The task type this processor handles. Must be a reference type.</typeparam>
/// <typeparam name="TResult">The result type produced. Must be a reference type.</typeparam>
public abstract class BackgroundTaskProcessor<TTask, TResult>
    where TTask : class
    where TResult : class
{
    public abstract TResult? Process(TTask task, ICopyPass copyPass);
}
