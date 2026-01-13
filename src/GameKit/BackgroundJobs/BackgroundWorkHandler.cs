namespace GameKit.BackgroundJobs;

/// <summary>
/// Factory interface for creating background work handlers.
/// Implement this to define how handlers are created, with dependencies injected via the constructor.
/// </summary>
/// <typeparam name="TMessage">The message type the handler processes.</typeparam>
public interface IHandlerFactory<TMessage>
    where TMessage : class
{
    BackgroundWorkHandler<TMessage> Create();
}

/// <summary>
/// Base class for handling background work messages. Implement this to define how a message type is processed.
/// Each worker thread creates its own instance via the factory registered with <see cref="BackgroundWorkerPool"/>.
/// </summary>
/// <typeparam name="TMessage">The message type this handler processes. Must be a reference type.</typeparam>
public abstract class BackgroundWorkHandler<TMessage>
    where TMessage : class
{
    /// <summary>
    /// Handles a message. Use the context for GPU resources and the hub to send messages.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="context">The context providing GPU access.</param>
    /// <param name="hub">The hub for sending messages to background or main threads.</param>
    public abstract void Handle(TMessage message, IBackgroundWorkContext context, BackgroundWorkHub hub);
}
