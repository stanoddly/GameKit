
namespace GameKit.DependencyInjection;

internal class ServiceTypeId : StaticTypeIdMap<ServiceTypeId>;

internal class ServiceTypeId<T> : StaticTypeIdMap<ServiceTypeId, T> where T : allows ref struct;
