namespace GameKit.BackgroundJobs;

/// <summary>
/// Handles messages on the main thread.
/// Register with <see cref="MainMessageDispatcher"/> to handle messages of a specific type.
/// </summary>
/// <typeparam name="TMessage">The message type to handle. Must be a reference type.</typeparam>
public interface IMainWorkHandler<TMessage>
    where TMessage : class
{
    /// <summary>
    /// Called on the main thread when a message is ready.
    /// </summary>
    void Handle(TMessage message);
}
