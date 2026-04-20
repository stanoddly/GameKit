using System.Runtime.CompilerServices;
using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public abstract class StatefulComponent<TSelf, TState> : GameComponent
    where TSelf : StatefulComponent<TSelf, TState>
    where TState : StatefulComponent<TSelf, TState>.State
{
    private TState _state;

    protected StatefulComponent(TState initialState) => _state = initialState;

    protected internal override void OnAttach(GameObject owner, ServiceProvider services) => _state.Enter(Unsafe.As<TSelf>(this));
    protected internal override void OnDetach(GameObject owner, ServiceProvider services) => _state.Exit(Unsafe.As<TSelf>(this));

    protected TState ChangeState(TState newState)
    {
        ArgumentNullException.ThrowIfNull(newState);
        TSelf self = Unsafe.As<TSelf>(this);
        TState oldState = _state;
        _state = newState;
        oldState.Exit(self);
        // Exit triggered a reentrant transition; that transition wins.
        if (_state != newState)
        {
            return _state;
        }
        newState.Enter(self);
        return newState;
    }

    protected TState CurrentState => _state;

    public abstract class State
    {
        public virtual void Enter(TSelf context) { }
        public virtual void Exit(TSelf context) { }
    }

}
