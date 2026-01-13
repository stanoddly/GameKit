using System.Runtime.CompilerServices;

namespace GameKit.BackgroundJobs;

internal abstract class MainHandlerWrapper
{
    public abstract void Handle(object message);
}

internal class MainHandlerWrapper<TMessage> : MainHandlerWrapper
    where TMessage : class
{
    private readonly IMainWorkHandler<TMessage> _handler;

    public MainHandlerWrapper(IMainWorkHandler<TMessage> handler)
    {
        _handler = handler;
    }

    // Unsafe.As is safe here: TypeId mechanism guarantees that messages dequeued with this TypeId
    // were produced by a handler registered with the same TMessage type.
    public override void Handle(object message)
    {
        _handler.Handle(Unsafe.As<TMessage>(message));
    }
}
