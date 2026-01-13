using System.Runtime.CompilerServices;

namespace GameKit.BackgroundJobs;

internal abstract class BackgroundHandlerWrapper
{
    public abstract void Handle(object message, IBackgroundWorkContext context, BackgroundWorkHub hub);
}

internal class BackgroundHandlerWrapper<TMessage> : BackgroundHandlerWrapper
    where TMessage : class
{
    private readonly BackgroundWorkHandler<TMessage> _handler;

    public BackgroundHandlerWrapper(BackgroundWorkHandler<TMessage> handler)
    {
        _handler = handler;
    }

    // Unsafe.As is safe here: messages are dispatched with TypeId matching this handler's MessageTypeId.
    public override void Handle(object message, IBackgroundWorkContext context, BackgroundWorkHub hub)
    {
        _handler.Handle(Unsafe.As<TMessage>(message), context, hub);
    }
}

internal abstract class HandlerRegistration
{
    public abstract int MessageTypeId { get; }
    public abstract BackgroundHandlerWrapper CreateWrapper();
}

internal class HandlerRegistration<TMessage> : HandlerRegistration
    where TMessage : class
{
    private readonly Func<BackgroundWorkHandler<TMessage>> _factory;

    public override int MessageTypeId => MessageTypeId<TMessage>.Id;

    public HandlerRegistration(Func<BackgroundWorkHandler<TMessage>> factory)
    {
        _factory = factory;
    }

    public override BackgroundHandlerWrapper CreateWrapper()
    {
        return new BackgroundHandlerWrapper<TMessage>(_factory());
    }
}
