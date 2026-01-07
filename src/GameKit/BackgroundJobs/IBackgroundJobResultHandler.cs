namespace GameKit.BackgroundJobs;

public interface IBackgroundJobResultHandler<TResult>
{
    void HandleResult(TResult result);
}
