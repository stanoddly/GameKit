
namespace Pixely.Events;

internal class TypeId : TypeIdMap<TypeId>;

internal class TypeId<T> : TypeIdMap<TypeId, T> where T : allows ref struct;
