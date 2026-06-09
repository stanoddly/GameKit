namespace GameKit.Architecture;

public interface ICommandDispatcher
{
    bool Dispatch<TCommand>(TCommand command);
}
