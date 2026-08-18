namespace Pixely.Utils;

public abstract class CommandHandler<TCommand>
{
    public abstract void Handle(TCommand command);
}

public abstract class QueryHandler<TQueryArg, TResult>
{
    public abstract TResult Handle(TQueryArg queryArg);
}
