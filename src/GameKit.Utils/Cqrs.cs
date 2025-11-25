namespace GameKit.Utils;

public abstract class CommandHandler<TCommand>
{
    public abstract void Handle(TCommand command);
}

public abstract class QueryHandler<TQuery, TResult>
{
    public abstract TResult Handle(TQuery query);
}
