using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameKit.Encs;

internal static class ComponentTypeHelper
{
    private static readonly Type TypeOfIEventHandler = typeof(IEventHandler<>);
    private static readonly Dictionary<Type, List<int>> Cache = new();

    // To avoid trimming
    // https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming?pivots=dotnet-8-0#dynamicallyaccessedmembers
    internal static List<int> GetComponentTypeHandledEventArgs<TComponent>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TComponent obj
    )
    {
        Type objectType = obj.GetType();
        ref List<int>? items = ref CollectionsMarshal.GetValueRefOrAddDefault(Cache, objectType, out bool exists);

        if (exists)
        {
            return items!;
        }

        items = [];

        foreach (var whateverInterface in objectType.GetInterfaces())
        {
            if (!whateverInterface.IsGenericType || whateverInterface.GetGenericTypeDefinition() != TypeOfIEventHandler)
                continue;

            Type[] genericArguments = whateverInterface.GetGenericArguments();
            Type eventArgsType = genericArguments[0];
            int typeId = TypeId.GetId(eventArgsType);
            items.Add(typeId);
        }

        return items;
    }
}

public class EventBus
{
    private readonly Dictionary<int, List<object>> _eventHandlersPerType = new();

    public void Subscribe<TSubscriber>(TSubscriber obj)
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

    public void Unsubscribe<TSubscriber>(TSubscriber obj)
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

    public void PublishEvent<TEventArgs>(in TEventArgs args) where TEventArgs: struct
    {
        int eventArgsTypeId = TypeId<TEventArgs>.Id;

        if (!_eventHandlersPerType.TryGetValue(eventArgsTypeId, out var subscriptions)) return;

        foreach (object obj in subscriptions)
        {
            IEventHandler<TEventArgs> eventHandler = Unsafe.As<IEventHandler<TEventArgs>>(obj);
            eventHandler.Handle(in args);
        }
    }
}