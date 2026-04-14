using GameKit.Common;

namespace GameKit.DependencyInjection;

internal class ServiceTypeId : TypeIdMap<ServiceTypeId>;

internal class ServiceTypeId<T> : TypeIdMap<ServiceTypeId, T> where T : allows ref struct;
