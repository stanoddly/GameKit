using System.Runtime.InteropServices;

namespace GameKit.Componentize;

public interface IComponentEventHandler;

public interface IComponentEventHandler<TEventArgs>: IComponentEventHandler where TEventArgs: struct
{
    void HandleEvent(GameObject gameObject, in TEventArgs args);
}

internal static class ComponentTypeHelper
{
    private static readonly Dictionary<Type, List<int>> Cache = new();

    public static List<int> GetComponentTypeHandledEventArgs(object obj)
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
            if (!whateverInterface.IsGenericType || whateverInterface.GetGenericTypeDefinition() != typeof(IComponentEventHandler<>))
                continue;

            Type[] genericArguments = whateverInterface.GetGenericArguments();
            Type eventArgsType = genericArguments[0];
            int typeId = EventTypeId.GetId(eventArgsType);
            items.Add(typeId);
        }

        return items;
    }
}
