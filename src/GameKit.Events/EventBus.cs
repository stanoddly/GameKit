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
    private readonly List<List<object?>?> _eventHandlersPerType = new();
    private int _publishDepth;
    private bool _hasDeferredRemovals;

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
            _eventHandlersPerType[eventArgsTypeId] ??= new List<object?>();
            _eventHandlersPerType[eventArgsTypeId]!.Add(instance);
        }
    }

    public void Subscribe<TEventArgs>(IEventHandler<TEventArgs> obj)
    {
        int id = TypeId<TEventArgs>.Id;
        EnsureCapacity(id);
        _eventHandlersPerType[id] ??= new List<object?>();
        _eventHandlersPerType[id]!.Add(obj);
    }

    public void Unsubscribe<TEventArgs>(IEventHandler<TEventArgs> obj)
    {
        int id = TypeId<TEventArgs>.Id;

        if (id >= _eventHandlersPerType.Count)
        {
            return;
        }

        RemoveHandler(_eventHandlersPerType[id], obj);
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

            RemoveHandler(_eventHandlersPerType[eventArgsTypeId], instance);
        }
    }

    private void RemoveHandler(List<object?>? subscriptions, object instance)
    {
        if (subscriptions == null)
        {
            return;
        }

        int index = subscriptions.IndexOf(instance);

        if (index < 0)
        {
            return;
        }

        if (_publishDepth == 0)
        {
            subscriptions.RemoveAt(index);
            return;
        }

        subscriptions[index] = null;
        _hasDeferredRemovals = true;
    }

    public void PublishEvent<TEventArgs>(TEventArgs args)
    {
        int eventArgsTypeId = TypeId<TEventArgs>.Id;

        if (eventArgsTypeId >= _eventHandlersPerType.Count)
        {
            return;
        }

        List<object?>? subscriptions = _eventHandlersPerType[eventArgsTypeId];

        if (subscriptions == null)
        {
            return;
        }

        int subscriptionCount = subscriptions.Count;

        _publishDepth++;

        try
        {
            for (int i = 0; i < subscriptionCount; i++)
            {
                object? obj = subscriptions[i];

                if (obj == null)
                {
                    continue;
                }

                IEventHandler<TEventArgs> eventHandler = Unsafe.As<IEventHandler<TEventArgs>>(obj);
                eventHandler.Process(args);
            }
        }
        finally
        {
            _publishDepth--;
            CompactDeferredRemovals();
        }
    }

    private void CompactDeferredRemovals()
    {
        if (_publishDepth > 0 || !_hasDeferredRemovals)
        {
            return;
        }

        foreach (List<object?>? subscriptions in _eventHandlersPerType)
        {
            if (subscriptions == null)
            {
                continue;
            }

            subscriptions.RemoveAll(static subscription => subscription == null);
        }

        _hasDeferredRemovals = false;
    }
}
