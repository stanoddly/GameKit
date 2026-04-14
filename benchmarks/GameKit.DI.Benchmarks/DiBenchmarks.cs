using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using GkDI = GameKit.DI;

namespace GameKit.DI.Benchmarks;

[MemoryDiagnoser]
public class BuildAndResolveAllBenchmark
{
    [Benchmark(Description = "GameKit.DI")]
    public GkDI.ServiceProvider GameKitDI()
    {
        GkDI.ServiceCollection collection = RegistrationHelper.BuildGameKitCollection();
        return collection.BuildServiceProvider();
    }

    [Benchmark(Description = "MEDI", Baseline = true)]
    public IServiceProvider Medi()
    {
        Microsoft.Extensions.DependencyInjection.ServiceCollection collection = RegistrationHelper.BuildMediCollection();
        IServiceProvider provider = collection.BuildServiceProvider();

        // MEDI resolves lazily — force all singletons to resolve
        provider.GetRequiredService<Leaf00>(); provider.GetRequiredService<Leaf01>(); provider.GetRequiredService<Leaf02>(); provider.GetRequiredService<Leaf03>(); provider.GetRequiredService<Leaf04>();
        provider.GetRequiredService<Leaf05>(); provider.GetRequiredService<Leaf06>(); provider.GetRequiredService<Leaf07>(); provider.GetRequiredService<Leaf08>(); provider.GetRequiredService<Leaf09>();
        provider.GetRequiredService<Leaf10>(); provider.GetRequiredService<Leaf11>(); provider.GetRequiredService<Leaf12>(); provider.GetRequiredService<Leaf13>(); provider.GetRequiredService<Leaf14>();
        provider.GetRequiredService<Leaf15>(); provider.GetRequiredService<Leaf16>(); provider.GetRequiredService<Leaf17>(); provider.GetRequiredService<Leaf18>(); provider.GetRequiredService<Leaf19>();
        provider.GetRequiredService<Chain00>(); provider.GetRequiredService<Chain01>(); provider.GetRequiredService<Chain02>(); provider.GetRequiredService<Chain03>(); provider.GetRequiredService<Chain04>();
        provider.GetRequiredService<Chain05>(); provider.GetRequiredService<Chain06>(); provider.GetRequiredService<Chain07>(); provider.GetRequiredService<Chain08>(); provider.GetRequiredService<Chain09>();
        provider.GetRequiredService<Chain10>(); provider.GetRequiredService<Chain11>(); provider.GetRequiredService<Chain12>(); provider.GetRequiredService<Chain13>(); provider.GetRequiredService<Chain14>();
        provider.GetRequiredService<Chain15>(); provider.GetRequiredService<Chain16>(); provider.GetRequiredService<Chain17>(); provider.GetRequiredService<Chain18>(); provider.GetRequiredService<Chain19>();
        provider.GetRequiredService<Fan00>(); provider.GetRequiredService<Fan01>(); provider.GetRequiredService<Fan02>(); provider.GetRequiredService<Fan03>(); provider.GetRequiredService<Fan04>();
        provider.GetRequiredService<Fan05>(); provider.GetRequiredService<Fan06>(); provider.GetRequiredService<Fan07>(); provider.GetRequiredService<Fan08>(); provider.GetRequiredService<Fan09>();
        provider.GetRequiredService<Fan10>(); provider.GetRequiredService<Fan11>(); provider.GetRequiredService<Fan12>(); provider.GetRequiredService<Fan13>(); provider.GetRequiredService<Fan14>();
        provider.GetRequiredService<Fan15>(); provider.GetRequiredService<Fan16>(); provider.GetRequiredService<Fan17>(); provider.GetRequiredService<Fan18>(); provider.GetRequiredService<Fan19>();
        provider.GetRequiredService<Diamond00>(); provider.GetRequiredService<Diamond01>(); provider.GetRequiredService<Diamond02>(); provider.GetRequiredService<Diamond03>(); provider.GetRequiredService<Diamond04>();
        provider.GetRequiredService<Diamond05>(); provider.GetRequiredService<Diamond06>(); provider.GetRequiredService<Diamond07>(); provider.GetRequiredService<Diamond08>(); provider.GetRequiredService<Diamond09>();
        provider.GetRequiredService<Diamond10>(); provider.GetRequiredService<Diamond11>(); provider.GetRequiredService<Diamond12>(); provider.GetRequiredService<Diamond13>(); provider.GetRequiredService<Diamond14>();
        provider.GetRequiredService<Diamond15>(); provider.GetRequiredService<Diamond16>(); provider.GetRequiredService<Diamond17>(); provider.GetRequiredService<Diamond18>(); provider.GetRequiredService<Diamond19>();
        provider.GetRequiredService<Top00>(); provider.GetRequiredService<Top01>(); provider.GetRequiredService<Top02>(); provider.GetRequiredService<Top03>(); provider.GetRequiredService<Top04>();
        provider.GetRequiredService<Top05>(); provider.GetRequiredService<Top06>(); provider.GetRequiredService<Top07>(); provider.GetRequiredService<Top08>(); provider.GetRequiredService<Top09>();
        provider.GetRequiredService<Top10>(); provider.GetRequiredService<Top11>(); provider.GetRequiredService<Top12>(); provider.GetRequiredService<Top13>(); provider.GetRequiredService<Top14>();
        provider.GetRequiredService<Top15>(); provider.GetRequiredService<Top16>(); provider.GetRequiredService<Top17>(); provider.GetRequiredService<Top18>(); provider.GetRequiredService<Top19>();

        return provider;
    }
}

[MemoryDiagnoser]
public class GetServiceBenchmark
{
    private GkDI.ServiceProvider _gameKitProvider = null!;
    private IServiceProvider _mediProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        GkDI.ServiceCollection gkCollection = RegistrationHelper.BuildGameKitCollection();
        _gameKitProvider = gkCollection.BuildServiceProvider();

        Microsoft.Extensions.DependencyInjection.ServiceCollection mediCollection = RegistrationHelper.BuildMediCollection();
        _mediProvider = mediCollection.BuildServiceProvider();
        // Pre-resolve all MEDI singletons
        _mediProvider.GetRequiredService<Top19>();
    }

    [Benchmark(Description = "GameKit.DI")]
    public Top19 GameKitDI()
    {
        return _gameKitProvider.GetService<Top19>();
    }

    [Benchmark(Description = "MEDI", Baseline = true)]
    public Top19 Medi()
    {
        return _mediProvider.GetRequiredService<Top19>();
    }
}

[MemoryDiagnoser]
public class GetServiceManyBenchmark
{
    private GkDI.ServiceProvider _gameKitProvider = null!;
    private IServiceProvider _mediProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        GkDI.ServiceCollection gkCollection = RegistrationHelper.BuildGameKitCollection();
        _gameKitProvider = gkCollection.BuildServiceProvider();

        Microsoft.Extensions.DependencyInjection.ServiceCollection mediCollection = RegistrationHelper.BuildMediCollection();
        _mediProvider = mediCollection.BuildServiceProvider();
        // Pre-resolve all MEDI singletons
        _mediProvider.GetRequiredService<Top19>();
        _mediProvider.GetRequiredService<Diamond10>();
        _mediProvider.GetRequiredService<Fan05>();
        _mediProvider.GetRequiredService<Chain10>();
        _mediProvider.GetRequiredService<Leaf00>();
    }

    [Benchmark(Description = "GameKit.DI")]
    public int GameKitDI()
    {
        // Resolve a mix of services across all tiers to simulate real access patterns
        int hash = 0;
        hash ^= _gameKitProvider.GetService<Leaf00>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Leaf10>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Chain05>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Chain15>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Fan03>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Fan13>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Diamond05>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Diamond15>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Top05>().GetHashCode();
        hash ^= _gameKitProvider.GetService<Top19>().GetHashCode();
        return hash;
    }

    [Benchmark(Description = "MEDI", Baseline = true)]
    public int Medi()
    {
        int hash = 0;
        hash ^= _mediProvider.GetRequiredService<Leaf00>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Leaf10>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Chain05>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Chain15>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Fan03>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Fan13>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Diamond05>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Diamond15>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Top05>().GetHashCode();
        hash ^= _mediProvider.GetRequiredService<Top19>().GetHashCode();
        return hash;
    }
}
