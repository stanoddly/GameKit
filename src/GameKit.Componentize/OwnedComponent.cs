using System.Runtime.CompilerServices;
using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public abstract class OwnedComponent : GameComponent
{
    private GameObject? _owner;
    private ServiceProvider? _serviceProvider;

    public GameObject Owner => _owner ?? throw new InvalidOperationException("Component has no owner. Attach it to a GameObject first.");

    public ServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("Component has no owner. Attach it to a GameObject first.");

    public bool HasOwner()
    {
        return _owner != null;
    }

    public GameWorld World
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ServiceProvider.GetRequiredService<GameWorld>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetRequiredService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetService<T>() where T : class
    {
        return ServiceProvider.GetService<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TComponent GetSibling<TComponent>() where TComponent : GameComponent
    {
        return Owner.Get<TComponent>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TComponent? TryGetSibling<TComponent>() where TComponent : GameComponent
    {
        return Owner.TryGet<TComponent>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AttachSibling<TComponent>(TComponent component) where TComponent : GameComponent
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
        _owner?.Detach(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachSibling<TComponent>() where TComponent : GameComponent
    {
        _owner?.Detach<TComponent>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachSibling(GameComponent component)
    {
        _owner?.Detach(component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveOwner()
    {
        if (_owner == null)
        {
            return;
        }
        World.RemoveGameObject(Owner);
    }

    protected internal sealed override void OnAttach(GameObject owner, ServiceProvider services)
    {
        _owner = owner;
        _serviceProvider = services;
        OnAttach();
    }

    protected internal sealed override void OnReady(GameObject owner, ServiceProvider services)
    {
        OnReady();
    }

    protected internal sealed override void OnDetach(GameObject owner, ServiceProvider services)
    {
        OnDetach();
        _owner = null;
        _serviceProvider = null;
    }

    protected virtual void OnAttach()
    {

    }

    protected virtual void OnReady()
    {

    }

    protected virtual void OnDetach()
    {

    }
}
