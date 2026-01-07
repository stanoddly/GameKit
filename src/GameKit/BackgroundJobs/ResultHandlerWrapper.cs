using System.Runtime.CompilerServices;

namespace GameKit.BackgroundJobs;

internal abstract class ResultHandlerWrapper
{
    public abstract void HandleResult(object result);
}

internal class ResultHandlerWrapper<TResult> : ResultHandlerWrapper
    where TResult : class
{
    private readonly IBackgroundJobResultHandler<TResult> _handler;

    public ResultHandlerWrapper(IBackgroundJobResultHandler<TResult> handler)
    {
        _handler = handler;
    }

    // Unsafe.As is safe here: TypeId mechanism guarantees that results dequeued with this TypeId
    // were produced by a processor registered with the same TResult type.
    public override void HandleResult(object result)
    {
        _handler.HandleResult(Unsafe.As<TResult>(result));
    }
}
