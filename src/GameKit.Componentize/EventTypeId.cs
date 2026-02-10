using GameKit.Common;

namespace GameKit.Componentize;

internal class EventTypeId : TypeIdMap<EventTypeId>;

internal class EventTypeId<TEventArgs> : TypeIdMap<EventTypeId, TEventArgs>;
