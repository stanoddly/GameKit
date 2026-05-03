using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.Events;

internal static class ComponentTypeHelper
{
    private static readonly Dictionary<Type, List<int>> Cache = new();

    internal static List<int> GetComponentTypeHandledEventArgs(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        ref List<int>? items = ref CollectionsMarshal.GetValueRefOrAddDefault(Cache, type, out bool exists);

        if (exists)
        {
            return items!;
        }

        items = new List<int>();

        foreach (Type candidateInterface in type.GetInterfaces())
        {
            if (!candidateInterface.IsGenericType)
            {
                continue;
            }

            if (candidateInterface.GetGenericTypeDefinition() != typeof(IEventHandler<>))
            {
                continue;
            }

            Type[] genericArguments = candidateInterface.GetGenericArguments();
            Type eventArgsType = genericArguments[0];
            int typeId = TypeId.GetId(eventArgsType);
            items.Add(typeId);
        }

        return items;
    }
}

internal class EventBus : IEventBus
{
    private readonly List<List<object>?> _eventHandlersPerType = new();

    private void EnsureCapacity(int id)
    {
        while (_eventHandlersPerType.Count <= id)
        {
            _eventHandlersPerType.Add(null);
        }
    }

    public void Subscribe<TSubscriber>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TSubscriber instance) where TSubscriber : notnull
    {
        Subscribe(instance, typeof(TSubscriber));
    }

    public void Subscribe(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        List<int> componentTypeHandledEventArgs = ComponentTypeHelper.GetComponentTypeHandledEventArgs(type);

        foreach (int eventArgsTypeId in componentTypeHandledEventArgs)
        {
            EnsureCapacity(eventArgsTypeId);
            _eventHandlersPerType[eventArgsTypeId] ??= new List<object>();
            _eventHandlersPerType[eventArgsTypeId]!.Add(instance);
        }
    }

    public void Subscribe<TEventArgs>(IEventHandler<TEventArgs> obj)
    {
        int id = TypeId<TEventArgs>.Id;
        EnsureCapacity(id);
        _eventHandlersPerType[id] ??= new List<object>();
        _eventHandlersPerType[id]!.Add(obj);
    }

    public void Unsubscribe<TEventArgs>(IEventHandler<TEventArgs> obj)
    {
        int id = TypeId<TEventArgs>.Id;

        if (id >= _eventHandlersPerType.Count)
        {
            return;
        }

        _eventHandlersPerType[id]?.Remove(obj);
    }

    public void Unsubscribe<TSubscriber>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TSubscriber instance) where TSubscriber : notnull
    {
        Unsubscribe(instance, typeof(TSubscriber));
    }

    public void Unsubscribe(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        List<int> componentTypeHandledEventArgs = ComponentTypeHelper.GetComponentTypeHandledEventArgs(type);
        foreach (int eventArgsTypeId in componentTypeHandledEventArgs)
        {
            if (eventArgsTypeId >= _eventHandlersPerType.Count)
            {
                continue;
            }

            _eventHandlersPerType[eventArgsTypeId]?.Remove(instance);
        }
    }

    public void PublishEvent<TEventArgs>(TEventArgs args)
    {
        int eventArgsTypeId = TypeId<TEventArgs>.Id;

        if (eventArgsTypeId >= _eventHandlersPerType.Count)
        {
            return;
        }

        List<object>? subscriptions = _eventHandlersPerType[eventArgsTypeId];

        if (subscriptions == null)
        {
            return;
        }

        foreach (object obj in subscriptions)
        {
            IEventHandler<TEventArgs> eventHandler = Unsafe.As<IEventHandler<TEventArgs>>(obj);
            eventHandler.Process(args);
        }
    }

    public void PublishEvents<TEventArgs>(ReadOnlySpan<TEventArgs> args)
    {
        int eventArgsTypeId = TypeId<TEventArgs>.Id;

        if (eventArgsTypeId >= _eventHandlersPerType.Count)
        {
            return;
        }

        List<object>? subscriptions = _eventHandlersPerType[eventArgsTypeId];

        if (subscriptions == null)
        {
            return;
        }

        foreach (TEventArgs arg in args)
        {
            foreach (object obj in subscriptions)
            {
                IEventHandler<TEventArgs> eventHandler = Unsafe.As<IEventHandler<TEventArgs>>(obj);
                eventHandler.Process(arg);
            }
        }
    }

    public void PublishEvents<TEventArgs>(List<TEventArgs> args)
    {
        int eventArgsTypeId = TypeId<TEventArgs>.Id;

        if (eventArgsTypeId >= _eventHandlersPerType.Count)
        {
            return;
        }

        List<object>? subscriptions = _eventHandlersPerType[eventArgsTypeId];

        if (subscriptions == null)
        {
            return;
        }

        foreach (TEventArgs arg in args)
        {
            foreach (object obj in subscriptions)
            {
                IEventHandler<TEventArgs> eventHandler = Unsafe.As<IEventHandler<TEventArgs>>(obj);
                eventHandler.Process(arg);
            }
        }
    }
}
