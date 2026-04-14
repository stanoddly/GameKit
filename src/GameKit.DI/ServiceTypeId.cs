using GameKit.Common;

namespace GameKit.DI;

internal class ServiceTypeId : TypeIdMap<ServiceTypeId>;

internal class ServiceTypeId<T> : TypeIdMap<ServiceTypeId, T> where T : allows ref struct;
