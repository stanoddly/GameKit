using System.Diagnostics;

namespace GameKit.BackgroundJobs;

/// <summary>
/// Delivers background job results to handlers on the main thread.
/// Implements <see cref="IUpdatable"/> and processes results each frame within a time budget.
/// </summary>
public class BackgroundJobResultDispatcher : IUpdatable
{
    private const long FrameBudgetMs = 2;

    private readonly BackgroundJobWorkerPool _workerPool;
    private readonly List<ResultHandlerWrapper?> _handlers = [null];
    private readonly Stopwatch _stopwatch = new();

    public BackgroundJobResultDispatcher(BackgroundJobWorkerPool workerPool)
    {
        _workerPool = workerPool;
    }

    public void RegisterHandler<TResult>(IBackgroundJobResultHandler<TResult> handler) where TResult : class
    {
        int typeId = BackgroundJobTypeId<TResult>.Id;

        while (_handlers.Count <= typeId)
        {
            _handlers.Add(null);
        }

        _handlers[typeId] = new ResultHandlerWrapper<TResult>(handler);
    }

    public void UnregisterHandler<TResult>() where TResult : class
    {
        int typeId = BackgroundJobTypeId<TResult>.Id;

        if (typeId < _handlers.Count)
        {
            _handlers[typeId] = null;
        }
    }

    public void Update()
    {
        _stopwatch.Restart();

        while (_stopwatch.ElapsedMilliseconds < FrameBudgetMs &&
               _workerPool.TryDequeueResult(out BackgroundJobResult result))
        {
            ResultHandlerWrapper? handler = result.TypeId < _handlers.Count ? _handlers[result.TypeId] : null;
            handler?.HandleResult(result.Result);
        }
    }
}
