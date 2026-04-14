using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using GameKit.DependencyInjection.Benchmarks;

ManualConfig config = ManualConfig.CreateMinimumViable()
    .AddLogger(ConsoleLogger.Default);

BenchmarkRunner.Run([
    typeof(BuildAndResolveAllBenchmark),
    typeof(GetServiceBenchmark),
    typeof(GetServiceManyBenchmark)
], config);
