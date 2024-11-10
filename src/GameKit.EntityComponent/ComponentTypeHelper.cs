using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace GameKit.EntityComponent;

internal static class ComponentTypeHelper
{
    private static readonly Type TypeOfIEventHandler = typeof(IEventHandler<>);
    private static readonly Dictionary<Type, List<int>> Cache = new();
    
    // To avoid trimming
    // https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming?pivots=dotnet-8-0#dynamicallyaccessedmembers
    internal static List<int> GetComponentTypeHandledEventArgs<TComponent>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TComponent obj
        ) where TComponent: GameComponent
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
