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

public class GameObject: IEnumerable<ComponentBase>
{
    internal Handle<GameObject> Handle { get; set; }
    public GameObjectState State { get; private set; }
    public event Action<GameObject>? Removed;
    internal readonly ServiceProvider ServiceProvider;
    private readonly List<ComponentBase> _components = new();

    internal GameObject(ServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    internal GameObject(ServiceProvider serviceProvider, List<ComponentBase> components)
    {
        ServiceProvider = serviceProvider;
        _components = components;
    }

    public TComponent Attach<TComponent>() where TComponent: ComponentBase, new()
    {
        return Attach(new TComponent());
    }

    public TComponent AttachIfMissing<TComponent>() where TComponent: ComponentBase, new()
    {
        TComponent? existing = TryGet<TComponent>();
        if (existing == null)
        {
            existing = Attach<TComponent>();
        }
        return existing;
    }

    public TComponent Attach<TComponent>(TComponent component) where TComponent: ComponentBase
    {
        if (State != GameObjectState.Alive)
        {
            throw new InvalidOperationException($"Cannot attach to {State} GameObject.");
        }

        _components.Add(component);
        component.OnAttach(this, ServiceProvider);
        component.OnReady(this, ServiceProvider);
        return component;
    }



    // Convenience method to be able to use []
    public void Add<TComponent>(TComponent component) where TComponent : ComponentBase
    {
        Attach(component);
    }

    public void Detach<TComponent>() where TComponent: ComponentBase
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

    public void DetachAll<TComponent>() where TComponent: ComponentBase
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

    public void Detach(ComponentBase component)
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

    internal void DetachAllForRemoval()
    {
        if (State == GameObjectState.Removed)
        {
            return;
        }

        if (_components.Count == 0)
        {
            return;
        }

        for (int i = _components.Count - 1; i >= 0; i--)
        {
            TeardownComponent(_components[i]);
        }

        _components.Clear();
    }

    public TComponent Get<TComponent>() where TComponent: ComponentBase
    {
        foreach (ComponentBase component in _components)
        {
            if (component is TComponent specificComponent)
            {
                return specificComponent;
            }
        }

        throw new ComponentNotFound(typeof(TComponent).Name);
    }

    public TComponent? TryGet<TComponent>() where TComponent: ComponentBase
    {
        foreach (ComponentBase component in _components)
        {
            if (component is TComponent specificComponent)
            {
                return specificComponent;
            }
        }

        return null;
    }

    public List<TComponent> GetComponents<TComponent>() where TComponent: ComponentBase
    {
        List<TComponent> components = new();
        foreach (ComponentBase component in _components)
        {
            if (component is TComponent specificComponent)
            {
                components.Add(specificComponent);
            }
        }
        return components;
    }


    public IEnumerator<ComponentBase> GetEnumerator()
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
    }

    private void TeardownComponent(ComponentBase component)
    {
        component.OnDetach(this, ServiceProvider);
    }
}
