using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.Componentize;

public readonly record struct ComponentRemovedArgs(GameComponent GameComponent);
public readonly record struct ComponentAddedArgs(GameComponent GameComponent);

public class ComponentNotFound(string componentName) : Exception(componentName);

public class GameObject: IEnumerable<GameComponent>
{
    public string Name { get; internal set; } = "nobody";
    public event Action<GameObject>? Removed;
    public GameWorld World { get; }
    private readonly List<GameComponent> _components = new();

    internal GameObject(GameWorld world)
    {
        World = world;
    }
    private Dictionary<int, List<object>>? _eventHandlersPerType = null;

    public void Subscribe(object obj)
    {
        List<int> componentTypeHandledEventArgs = ComponentTypeHelper.GetComponentTypeHandledEventArgs(obj);

        if (componentTypeHandledEventArgs.Count == 0)
        {
            return;
        }
        
        _eventHandlersPerType ??= new();

        foreach (int eventArgsTypeId in componentTypeHandledEventArgs)
        {
            ref List<object>? value = ref CollectionsMarshal.GetValueRefOrAddDefault(_eventHandlersPerType, eventArgsTypeId, out bool exists);

            if (!exists || value == null)
            {
                value = new List<object>();
            }
            
            value.Add(obj);
        }
    }

    public void Unsubscribe<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TComponent>(TComponent component) where TComponent: GameComponent
    {
        Unsubscribe((object)component);
    }
    
    public void Subscribe<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TComponent>(TComponent component) where TComponent: GameComponent
    {
        Subscribe((object)component);
    }

    public void Unsubscribe(object obj)
    {
        List<int> componentTypeHandledEventArgs = ComponentTypeHelper.GetComponentTypeHandledEventArgs(obj);

        if (componentTypeHandledEventArgs.Count == 0)
        {
            return;
        }

        if (_eventHandlersPerType == null)
        {
            return;
        }
        
        foreach (var whateverInterface in componentTypeHandledEventArgs)
        {
            if (!_eventHandlersPerType.TryGetValue(whateverInterface, out List<object>? value))
            {
                return;
            }

            value.Remove(obj);
        }
    }

    public GameObject Attach<TComponent>() where TComponent: GameComponent, new()
    {
        return Attach(new TComponent());
    }

    public GameObject AttachIfMissing<TComponent>() where TComponent: GameComponent, new()
    {
        if (TryGet<TComponent>() == null)
        {
            Attach<TComponent>();
        }
        return this;
    }

    public GameObject AttachIfMissing<TComponent>(out TComponent component) where TComponent: GameComponent, new()
    {
        TComponent? existing = TryGet<TComponent>();
        if (existing == null)
        {
            existing = new TComponent();
            Attach(existing);
        }
        component = existing;
        return this;
    }

    public GameObject Attach<TComponent>(TComponent component) where TComponent: GameComponent
    {
        component.InternalOwner = this;
        component.OnAttach();
        Subscribe(component);
        _components.Add(component);
        PublishEvent(new ComponentAddedArgs(component));
        World.NotifyComponentAttached(this, component);
        return this;
    }



    // Convenience method to be able to use []
    public void Add<TComponent>(TComponent component) where TComponent : GameComponent
    {
        Attach(component);
    }

    public void Detach<TComponent>() where TComponent: GameComponent
    {
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

    internal void NotifyRemoved()
    {
        Removed?.Invoke(this);
        Removed = null;
    }

    private void TeardownComponent(GameComponent component)
    {
        component.OnDetach();
        PublishEvent(new ComponentRemovedArgs(component));
        Unsubscribe(component);
        World.NotifyComponentDetached(this, component);
        component.InternalOwner = null;
    }

    internal void PublishEvent<TEventArgs>(in TEventArgs args) where TEventArgs: struct
    {
        if (_eventHandlersPerType == null)
        {
            return;
        }
        
        int eventArgsTypeId = EventTypeId<TEventArgs>.Id;

        if (!_eventHandlersPerType.TryGetValue(eventArgsTypeId, out var subscriptions)) return;

        foreach (object obj in subscriptions)
        {
            IComponentEventHandler<TEventArgs> componentEventHandler = Unsafe.As<IComponentEventHandler<TEventArgs>>(obj);
            componentEventHandler.HandleEvent(this, in args);
        }
    }
}
