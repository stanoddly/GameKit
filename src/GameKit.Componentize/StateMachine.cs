using System.Runtime.CompilerServices;

namespace GameKit.Componentize;

public abstract class StateMachine<TSelf, TState> : GameComponent
    where TSelf : StateMachine<TSelf, TState>
    where TState : StateMachine<TSelf, TState>.State
{
    private TState _state;

    protected StateMachine(TState initialState) => _state = initialState;

    protected internal override void OnAttach() => _state.Enter(Unsafe.As<TSelf>(this));
    protected internal override void OnDetach() => _state.Exit(Unsafe.As<TSelf>(this));

    public void Handle<TCommand>(in TCommand command) where TCommand : struct
    {
        if (_state is ICommandHandler<TCommand> handler)
        {
            handler.Handle(Unsafe.As<TSelf>(this), in command);
        }
    }

    protected TState ChangeState(TState newState)
    {
        TSelf self = Unsafe.As<TSelf>(this);
        _state.Exit(self);
        _state = newState;
        _state.Enter(self);
        return newState;
    }

    protected TState CurrentState => _state;

    public abstract class State
    {
        public virtual void Enter(TSelf context) { }
        public virtual void Exit(TSelf context) { }
    }

    public interface ICommandHandler<TCommand> where TCommand : struct
    {
        void Handle(TSelf context, in TCommand command);
    }
}
