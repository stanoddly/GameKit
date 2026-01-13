namespace GameKit.BackgroundJobs;

internal readonly record struct MainMessage(int TypeId, object Payload);
