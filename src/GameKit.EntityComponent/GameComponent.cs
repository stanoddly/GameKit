using System.Runtime.CompilerServices;

namespace GameKit.EntityComponent;

public abstract class GameComponent
{
    internal GameObject? InternalOwner = null;
    protected GameObject Owner => InternalOwner ?? throw new InvalidOperationException("missing parent");

    public bool HasOwner()
    {
        return InternalOwner != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TComponent GetSiblingOrFail<TComponent>() where TComponent: GameComponent
    {
        return Owner.GetOrFail<TComponent>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TComponent GetSiblingOrNew<TComponent>() where TComponent: GameComponent, new()
    {
        return Owner.GetOrNew<TComponent>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TComponent GetSiblingOrFactory<TComponent>(Func<TComponent> factory) where TComponent: GameComponent, new()
    {
        return Owner.GetOrFactory<TComponent>(factory);
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

    protected internal virtual void OnAttach()
    {
        
    }

    protected internal virtual void OnDetach()
    {
        
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static TService GetService<TService>() where TService: class
    {
        return ServiceLocator.GetService<TService>();
    }

    protected void PublishEvent<TEventArgs>(in TEventArgs args) where TEventArgs: struct
    {
        InternalOwner?.PublishEvent(in args);
    }
}
