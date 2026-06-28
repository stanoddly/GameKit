namespace GameKit.Architecture;

/// <summary>
/// Handles a requested mutation.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<TCommand>
{
    /// <summary>
    /// Returns <see langword="true"/> when the command is accepted and its requested postcondition holds.
    /// Returns <see langword="false"/> for an expected domain rejection that does not apply the requested
    /// state change. Invalid program state and infrastructure failures are reported with exceptions.
    /// </summary>
    bool Handle(TCommand command);
}
