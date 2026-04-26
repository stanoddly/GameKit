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

public class EventBus
{
    // TODO: this can be a slot map, but a slot map should work with uint first
    private readonly Dictionary<int, List<object>> _eventHandlersPerType = new();

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
            ref List<object>? value = ref CollectionsMarshal.GetValueRefOrAddDefault(_eventHandlersPerType, eventArgsTypeId, out bool exists);

            if (!exists || value == null)
            {
                value = new List<object>();
            }

            value.Add(instance);
        }
    }

    public void Subscribe<TEventArgs>(IEventHandler<TEventArgs> obj)
    {
        int id = TypeId<TEventArgs>.Id;
        ref List<object>? value = ref CollectionsMarshal.GetValueRefOrAddDefault(_eventHandlersPerType, id, out bool exists);

        if (!exists || value == null)
        {
            value = new List<object>();
        }

        value.Add(obj);
    }

    public void Unsubscribe<TEventArgs>(IEventHandler<TEventArgs> obj)
    {
        int id = TypeId<TEventArgs>.Id;

        if (!_eventHandlersPerType.TryGetValue(id, out List<object>? value))
        {
            return;
        }

        value.Remove(obj);
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
            if (!_eventHandlersPerType.TryGetValue(eventArgsTypeId, out List<object>? value))
            {
                continue;
            }

            value.Remove(instance);
        }
    }

    public void PublishEvent<TEventArgs>(TEventArgs args)
    {
        int eventArgsTypeId = TypeId<TEventArgs>.Id;

        if (!_eventHandlersPerType.TryGetValue(eventArgsTypeId, out List<object>? subscriptions))
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

        if (!_eventHandlersPerType.TryGetValue(eventArgsTypeId, out List<object>? subscriptions))
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

        if (!_eventHandlersPerType.TryGetValue(eventArgsTypeId, out List<object>? subscriptions))
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
