using GameKit.Common;

namespace GameKit.BackgroundJobs;

internal class MessageTypeId : TypeIdMap<MessageTypeId>;

internal class MessageTypeId<T> : TypeIdMap<MessageTypeId, T>;
