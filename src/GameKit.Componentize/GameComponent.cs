using System.Runtime.CompilerServices;

namespace GameKit.Componentize;

public abstract class GameComponent
{
    internal GameObject? InternalOwner = null;
    protected GameObject Owner => InternalOwner ?? throw new InvalidOperationException("Component has no owner. Attach it to a GameObject first.");

    public bool HasOwner()
    {
        return InternalOwner != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TComponent GetSibling<TComponent>() where TComponent: GameComponent
    {
        return Owner.Get<TComponent>();
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TComponent? TryGetSibling<TComponent>() where TComponent: GameComponent
    {
        return Owner.TryGet<TComponent>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AttachSibling<TComponent>(TComponent component) where TComponent: GameComponent
    {
        Owner.Attach(component);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Detach()
    {
        Owner.Detach(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachSibling<TComponent>() where TComponent: GameComponent
    {
        Owner.Detach<TComponent>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachSibling(GameComponent component)
    {
        Owner.Detach(component);
    }

    protected internal virtual void OnAttach()
    {
        
    }

    protected internal virtual void OnDetach()
    {
        
    }

    protected void PublishEvent<TEventArgs>(in TEventArgs args) where TEventArgs: struct
    {
        InternalOwner?.PublishEvent(in args);
    }
}
