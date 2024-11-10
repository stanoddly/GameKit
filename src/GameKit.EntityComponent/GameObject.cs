using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.EntityComponent;

public readonly record struct ComponentRemovedArgs(GameComponent GameComponent);
public readonly record struct ComponentAddedArgs(GameComponent GameComponent);

public class ComponentNotFound : Exception
{
    public ComponentNotFound(string componentName) : base(componentName)
    {
    }
}

public class GameObject: IEnumerable<GameComponent>
{
    private readonly Dictionary<int, GameComponent> _components = new();
    private readonly Dictionary<int, List<object>> _eventHandlersPerType = new();

    public void Subscribe<TComponent>(TComponent obj) where TComponent: GameComponent
    {
        List<int> componentTypeHandledEventArgs = ComponentTypeHelper.GetComponentTypeHandledEventArgs(obj);

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

    public void Unsubscribe<TComponent>(TComponent obj) where TComponent: GameComponent
    {
        List<int> componentTypeHandledEventArgs = ComponentTypeHelper.GetComponentTypeHandledEventArgs(obj);
        foreach (var whateverInterface in componentTypeHandledEventArgs)
        {
            if (!_eventHandlersPerType.TryGetValue(whateverInterface, out List<object>? value))
            {
                continue;
            }

            value.Remove(obj);
        }
    }

    public void Attach<TComponent>(TComponent component) where TComponent: GameComponent
    {
        int id = TypeId<TComponent>.Id;
        ref GameComponent? value = ref CollectionsMarshal.GetValueRefOrAddDefault(_components, id, out bool exists);

        if (exists && value != null)
        {
            Unsubscribe(value);

            value.OnDetach();
            value.InternalOwner = null;
            PublishEvent(new ComponentRemovedArgs(value));
        }

        value = component;
        component.InternalOwner = this;
        component.OnAttach();
        Subscribe(component);
        PublishEvent(new ComponentAddedArgs(component));
    }

    // Convenience method to be able to use []
    public void Add<TComponent>(TComponent component) where TComponent : GameComponent
    {
        Attach<TComponent>(component);
    }

    public void Detach<TComponent>() where TComponent: GameComponent
    {
        int id = TypeId<TComponent>.Id;
        if (_components.Remove(id, out GameComponent? component))
        {
            component.OnDetach();
            Unsubscribe(component);
            component.InternalOwner = null;
        }
    }

    public void DetachAll()
    {
        foreach ((_, GameComponent component) in _components)
        {
            component.OnDetach();
            Unsubscribe(component);
            component.InternalOwner = null;
        }
        // TODO: make sure OnDetach didn't create new components
        _components.Clear();
    }

    public TComponent GetOrFail<TComponent>() where TComponent: GameComponent
    {
        int id = TypeId<TComponent>.Id;
        try
        {
            return Unsafe.As<TComponent>(_components[id]);
        }
        catch (KeyNotFoundException)
        {
            throw new ComponentNotFound(TypeId<TComponent>.Name);
        }
    }
    
    public TComponent? TryGet<TComponent>() where TComponent: GameComponent
    {
        int id = TypeId<TComponent>.Id;
        if (_components.TryGetValue(id, out GameComponent? gameComponent))
        {
            return Unsafe.As<TComponent>(gameComponent);
        }

        return null;
    }
    
    public TComponent GetOrFactory<TComponent>(Func<TComponent> factory) where TComponent : GameComponent
    {
        int id = TypeId<TComponent>.Id;
        // value is "guaranteed" not to be null
        ref GameComponent value = ref CollectionsMarshal.GetValueRefOrAddDefault(_components, id, out bool exists)!;

        if (exists)
        {
            return Unsafe.As<TComponent>(value);
        }

        TComponent component = factory();
        value = component;
        component.InternalOwner = this;
        component.OnAttach();
        Subscribe(component);
        PublishEvent(new ComponentAddedArgs(component));

        return component;
    }
    
    public TComponent GetOrNew<TComponent>() where TComponent : GameComponent, new()
    {
        return GetOrNew<TComponent, TComponent>();
    }
    
    public TComponent GetOrNew<TComponent, TNewComponent>()
        where TComponent : GameComponent
        where TNewComponent: TComponent, new()
    {
        int id = TypeId<TComponent>.Id;
        // value is "guaranteed" not to be null
        ref GameComponent value = ref CollectionsMarshal.GetValueRefOrAddDefault(_components, id, out bool exists)!;

        if (exists)
        {
            return Unsafe.As<TComponent>(value);
        }

        TNewComponent component = new();
        value = component;
        component.InternalOwner = this;
        component.OnAttach();
        Subscribe(component);
        PublishEvent(new ComponentAddedArgs(component));

        return component;
    }

    public IEnumerator<GameComponent> GetEnumerator()
    {
        foreach (GameComponent component in _components.Values)
        {
            yield return component;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal void PublishEvent<TEventArgs>(in TEventArgs args) where TEventArgs: struct
    {
        int eventArgsTypeId = TypeId<TEventArgs>.Id;

        if (!_eventHandlersPerType.TryGetValue(eventArgsTypeId, out var subscriptions)) return;

        foreach (object obj in subscriptions)
        {
            IEventHandler<TEventArgs> eventHandler = Unsafe.As<IEventHandler<TEventArgs>>(obj);
            eventHandler.Handle(this, in args);
        }
    }

    public bool Handle<TCommand>(in TCommand command) where TCommand : struct
    {
        if (TryGet<StateComponent>() is StateComponent stateComponent)
        {
            return stateComponent.InternalHandle(in command);
        }

        return false;
    }
}
