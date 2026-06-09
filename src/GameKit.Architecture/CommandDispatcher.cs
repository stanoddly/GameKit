using GameKit.DependencyInjection;

namespace GameKit.Architecture;

/// <summary>
/// Resolves the registered <see cref="ICommandHandler{TCommand}"/> for each command and invokes it. After the
/// top-level command in a batch completes, every <see cref="IPostDispatchHook"/> runs once — re-entrant
/// commands dispatched by a handler share the same batch and do not re-trigger the hooks.
/// </summary>
public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly ServiceProvider _services;
    private readonly IPostDispatchHook[] _postDispatchHooks;
    private int _dispatchDepth;

    public CommandDispatcher(ServiceProvider services, IEnumerable<IPostDispatchHook> postDispatchHooks)
    {
        _services = services;
        _postDispatchHooks = postDispatchHooks.ToArray();
    }

    public bool Dispatch<TCommand>(TCommand command)
    {
        _dispatchDepth++;
        try
        {
            ICommandHandler<TCommand> handler = _services.GetRequiredService<ICommandHandler<TCommand>>();
            bool handled = handler.Handle(command);
            if (_dispatchDepth == 1)
            {
                foreach (IPostDispatchHook hook in _postDispatchHooks)
                {
                    hook.OnBatchCompleted();
                }
            }

            return handled;
        }
        finally
        {
            _dispatchDepth--;
        }
    }
}
