namespace GameKit.Componentize;

public interface ICommandHandler<TCommand> where TCommand : struct
{
    void Handle(in TCommand command);
}
