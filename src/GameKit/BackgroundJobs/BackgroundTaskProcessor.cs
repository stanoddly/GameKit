namespace GameKit.BackgroundJobs;

/// <summary>
/// Factory interface for creating background task processors.
/// Implement this to define how processors are created, with dependencies injected via the constructor.
/// </summary>
/// <typeparam name="TTask">The task type the processor handles.</typeparam>
public interface IProcessorFactory<TTask>
    where TTask : class
{
    BackgroundTaskProcessor<TTask> Create();
}

/// <summary>
/// Base class for processing background jobs. Implement this to define how a task type is processed.
/// Each worker thread creates its own instance via the factory registered with <see cref="BackgroundJobWorkerPool"/>.
/// </summary>
/// <typeparam name="TTask">The task type this processor handles. Must be a reference type.</typeparam>
public abstract class BackgroundTaskProcessor<TTask>
    where TTask : class
{
    /// <summary>
    /// Processes a task. Use the context to dispatch results and/or new jobs.
    /// </summary>
    /// <param name="task">The task to process.</param>
    /// <param name="context">The context providing GPU access and dispatch capabilities.</param>
    public abstract void Process(TTask task, IBackgroundJobContext context);
}
