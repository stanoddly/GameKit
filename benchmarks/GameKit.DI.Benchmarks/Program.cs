using BenchmarkDotNet.Running;
using GameKit.DI.Benchmarks;

BenchmarkRunner.Run([
    typeof(BuildAndResolveAllBenchmark),
    typeof(GetServiceBenchmark),
    typeof(GetServiceManyBenchmark)
]);
