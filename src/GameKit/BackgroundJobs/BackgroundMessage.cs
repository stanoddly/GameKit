namespace GameKit.BackgroundJobs;

internal readonly record struct BackgroundMessage(int TypeId, object Payload);
