namespace GameKit.BackgroundJobs;

internal readonly record struct BackgroundJob(int TypeId, object Task);
