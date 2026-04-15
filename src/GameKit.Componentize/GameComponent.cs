using System.Runtime.CompilerServices;
using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public abstract class GameComponent
{
    internal GameObject? InternalOwner = null;
    public GameObject Owner => InternalOwner ?? throw new InvalidOperationException("Component has no owner. Attach it to a GameObject first.");
    public GameWorld World => Owner.World;

    public bool HasOwner()
    {
        return InternalOwner != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetRequiredService<T>() where T : class
    {
        return World.ServiceProvider.GetRequiredService<T>();
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
    public TComponent AttachSiblingIfMissing<TComponent>() where TComponent : GameComponent, new()
    {
        return Owner.AttachIfMissing<TComponent>();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Detach()
    {
        InternalOwner?.Detach(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachSibling<TComponent>() where TComponent: GameComponent
    {
        InternalOwner?.Detach<TComponent>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachSibling(GameComponent component)
    {
        InternalOwner?.Detach(component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveOwner()
    {
        World.RemoveGameObject(Owner);
    }

    protected internal virtual void OnAttach()
    {

    }

    protected internal virtual void OnReady()
    {

    }

    protected internal virtual void OnDetach()
    {

    }
}
