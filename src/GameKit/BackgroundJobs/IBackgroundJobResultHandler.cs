namespace GameKit.BackgroundJobs;

/// <summary>
/// Receives background job results on the main thread.
/// Register with <see cref="BackgroundJobResultDispatcher"/> to handle results of a specific type.
/// </summary>
/// <typeparam name="TResult">The result type to handle. Must be a reference type.</typeparam>
public interface IBackgroundJobResultHandler<TResult>
    where TResult : class
{
    /// <summary>
    /// Called on the main thread when a result is ready.
    /// </summary>
    void HandleResult(TResult result);
}
