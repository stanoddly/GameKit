using GameKit.Gpu;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Provides context for background job processing, including GPU access and the ability to dispatch results and new jobs.
/// </summary>
public interface IBackgroundJobContext
{
    /// <summary>
    /// Gets the copy pass for GPU memory transfers.
    /// </summary>
    ICopyPass CopyPass { get; }

    /// <summary>
    /// Dispatches a result to be handled on the main thread.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The result to dispatch.</param>
    void DispatchResult<TResult>(TResult result) where TResult : class;

    /// <summary>
    /// Dispatches a new job to be processed by a background worker.
    /// </summary>
    /// <typeparam name="TTask">The task type.</typeparam>
    /// <param name="task">The task to dispatch.</param>
    /// <param name="priority">The priority level (0 = highest).</param>
    void DispatchJob<TTask>(TTask task, int priority = 0) where TTask : class;
}
