namespace Pixely.Architecture;

/// <summary>
/// Handles a requested mutation.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<TCommand>
{
    /// <summary>
    /// Returns <see cref="CommandResult.Success"/> when the command is accepted and its requested postcondition
    /// holds. Returns an error via <see cref="CommandResult.FromError"/> for an expected domain rejection that
    /// does not apply the requested state change. Invalid program state and infrastructure failures are reported
    /// with exceptions.
    /// </summary>
    CommandResult Handle(TCommand command);
}
