
namespace Pixely.Componentize;

internal class ComponentTypeId : TypeIdMap<ComponentTypeId>;

internal class ComponentTypeId<T> : TypeIdMap<ComponentTypeId, T> where T : GameComponent;
