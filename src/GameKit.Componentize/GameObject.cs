using System.Collections;
using GameKit.Collections;
using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public class ComponentNotFound(string componentName) : Exception(componentName);

public enum GameObjectState : byte
{
    Alive,
    Removing,
    Removed
}

public class GameObject: IEnumerable<GameComponent>
{
    internal Handle<GameObject> Handle { get; set; }
    public GameObjectState State { get; private set; }
    public event Action<GameObject>? Removed;
    internal ServiceProvider? InternalServiceProvider;
    private List<GameComponent> _components = new();

    internal GameObject(ServiceProvider serviceProvider)
    {
        InternalServiceProvider = serviceProvider;
    }

    internal GameObject(ServiceProvider serviceProvider, List<GameComponent> components)
    {
        InternalServiceProvider = serviceProvider;
        _components = components;
    }

    public TComponent Attach<TComponent>() where TComponent: GameComponent, new()
    {
        return Attach(new TComponent());
    }

    public TComponent AttachIfMissing<TComponent>() where TComponent: GameComponent, new()
    {
        TComponent? existing = TryGet<TComponent>();
        if (existing == null)
        {
            existing = Attach<TComponent>();
        }
        return existing;
    }

    public TComponent Attach<TComponent>(TComponent component) where TComponent: GameComponent
    {
        if (State != GameObjectState.Alive)
        {
            throw new InvalidOperationException($"Cannot attach to {State} GameObject.");
        }

        component.InternalOwner = this;
        _components.Add(component);
        component.OnAttach();
        component.OnReady();
        return component;
    }



    // Convenience method to be able to use []
    public void Add<TComponent>(TComponent component) where TComponent : GameComponent
    {
        Attach(component);
    }

    public void Detach<TComponent>() where TComponent: GameComponent
    {
        if (State != GameObjectState.Alive)
        {
            return;
        }

        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] is TComponent component)
            {
                TeardownComponent(component);
                _components.RemoveAt(i);
                return;
            }
        }
    }

    public void DetachAll<TComponent>() where TComponent: GameComponent
    {
        if (State != GameObjectState.Alive)
        {
            return;
        }

        for (int i = _components.Count - 1; i >= 0; i--)
        {
            if (_components[i] is TComponent component)
            {
                TeardownComponent(component);
                _components.RemoveAt(i);
            }
        }
    }

    public void Detach(GameComponent component)
    {
        if (State != GameObjectState.Alive)
        {
            return;
        }

        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] == component)
            {
                TeardownComponent(component);
                _components.RemoveAt(i);
                return;
            }
        }
    }

    public void DetachAll()
    {
        if (State == GameObjectState.Removed)
        {
            return;
        }

        if (_components.Count == 0)
        {
            return;
        }

        GameComponent[] snapshot = _components.ToArray();
        _components.Clear();

        foreach (GameComponent component in snapshot)
        {
            TeardownComponent(component);
        }
    }

    public TComponent Get<TComponent>() where TComponent: GameComponent
    {
        foreach (GameComponent component in _components)
        {
            if (component is TComponent specificComponent)
            {
                return specificComponent;
            }
        }

        throw new ComponentNotFound(typeof(TComponent).Name);
    }
    
    public TComponent? TryGet<TComponent>() where TComponent: GameComponent
    {
        foreach (GameComponent component in _components)
        {
            if (component is TComponent specificComponent)
            {
                return specificComponent;
            }
        }

        return null;
    }

    public List<TComponent> GetComponents<TComponent>() where TComponent: GameComponent
    {
        List<TComponent> components = new();
        foreach (GameComponent component in _components)
        {
            if (component is TComponent specificComponent)
            {
                components.Add(specificComponent);
            }
        }
        return components;
    }


    public IEnumerator<GameComponent> GetEnumerator()
    {
        for (int i = 0; i < _components.Count; i++)
        {
            yield return _components[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal void NotifyRemoving()
    {
        State = GameObjectState.Removing;
    }

    internal void NotifyRemoved()
    {
        Removed?.Invoke(this);
        Removed = null;
        State = GameObjectState.Removed;
        InternalServiceProvider = null;
    }

    private void TeardownComponent(GameComponent component)
    {
        component.OnDetach();
        component.InternalOwner = null;
    }
}
