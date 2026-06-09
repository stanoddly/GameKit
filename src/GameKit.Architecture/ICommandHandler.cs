namespace GameKit.Architecture;

public interface ICommandHandler<TCommand>
{
    bool Handle(TCommand command);
}
