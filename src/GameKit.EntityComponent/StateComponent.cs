using System.Runtime.CompilerServices;

namespace GameKit.EntityComponent;

public abstract class StateComponent: GameComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool InternalHandle<TCommand>(in TCommand command) where TCommand : struct
    {
        return Handle(in command);
    } 
    protected abstract bool Handle<TCommand>(in TCommand command) where TCommand : struct;

    protected bool ReplaceAndHandle<TCommand>(StateComponent stateComponent, in TCommand command) where TCommand : struct
    {
        AttachSibling(stateComponent);
        return stateComponent.InternalHandle(in command);
    }
    
    protected bool Replace(StateComponent stateComponent)
    {
        AttachSibling(stateComponent);
        return false;
    }
}
