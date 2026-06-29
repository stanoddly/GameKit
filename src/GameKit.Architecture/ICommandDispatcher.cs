namespace GameKit.Architecture;

/// <summary>
/// Dispatches commands and returns their handlers' acceptance results.
/// </summary>
public interface ICommandDispatcher
{
    /// <inheritdoc cref="ICommandHandler{TCommand}.Handle(TCommand)"/>
    CommandResult Dispatch<TCommand>(TCommand command);
}
