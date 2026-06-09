using GameKit.DependencyInjection;

namespace GameKit.Architecture;

/// <summary>
/// Resolves the registered <see cref="ICommandHandler{TCommand}"/> for each command and invokes it. After the
/// top-level command in a batch completes, every <see cref="ICommandDispatchHook"/> runs once, in registration
/// order — re-entrant commands dispatched by a handler share the same batch and do not re-trigger the hooks.
/// A hook that publishes domain events must be registered before any hook that drains them.
/// </summary>
public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly ServiceProvider _services;
    private readonly ICommandDispatchHook[] _dispatchHooks;
    private int _dispatchDepth;

    public CommandDispatcher(ServiceProvider services, IEnumerable<ICommandDispatchHook> dispatchHooks)
    {
        _services = services;
        _dispatchHooks = dispatchHooks.ToArray();
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
                foreach (ICommandDispatchHook hook in _dispatchHooks)
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
