using GameKit.DependencyInjection;

namespace GameKit.DependencyInjection.Tests;

public class SimpleService;

public class AnotherService;

public interface IMyService;

public class MyServiceImpl : IMyService;

public class AnotherServiceImpl : IMyService;

public interface IUnrelated;

public class ServiceWithDependency
{
    public SimpleService Simple { get; }

    public ServiceWithDependency(SimpleService simple)
    {
        Simple = simple;
    }
}

public class ServiceWithTwoDependencies
{
    public SimpleService Simple { get; }
    public AnotherService Another { get; }

    public ServiceWithTwoDependencies(SimpleService simple, AnotherService another)
    {
        Simple = simple;
        Another = another;
    }
}

public class CircularServiceA
{
    public CircularServiceB B { get; }
    public CircularServiceA(CircularServiceB b) => B = b;
}

public class CircularServiceB
{
    public CircularServiceA A { get; }
    public CircularServiceB(CircularServiceA a) => A = a;
}

public class MultiConstructorService
{
    public MultiConstructorService() { }
    public MultiConstructorService(SimpleService simple) { }
}

public class DisposableService : IDisposable
{
    public bool Disposed { get; private set; }
    public void Dispose() => Disposed = true;
}

public class ServiceCollectionTests
{
    // --- AddSingleton<T>() ---

    [Test]
    public void AddSingleton_ResolvesService()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(provider.GetService<SimpleService>(), Is.Not.Null);
    }

    [Test]
    public void AddSingleton_ReturnsSameInstance()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        SimpleService first = provider.GetService<SimpleService>();
        SimpleService second = provider.GetService<SimpleService>();

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void AddSingleton_WithDependency_ResolvesDependency()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        collection.AddSingleton<ServiceWithDependency>();

        ServiceProvider provider = collection.BuildServiceProvider();

        ServiceWithDependency service = provider.GetService<ServiceWithDependency>();

        Assert.That(service.Simple, Is.SameAs(provider.GetService<SimpleService>()));
    }

    [Test]
    public void AddSingleton_WithMultipleDependencies_ResolvesAll()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        collection.AddSingleton<AnotherService>();
        collection.AddSingleton<ServiceWithTwoDependencies>();

        ServiceProvider provider = collection.BuildServiceProvider();

        ServiceWithTwoDependencies service = provider.GetService<ServiceWithTwoDependencies>();

        Assert.That(service.Simple, Is.SameAs(provider.GetService<SimpleService>()));
        Assert.That(service.Another, Is.SameAs(provider.GetService<AnotherService>()));
    }

    [Test]
    public void AddSingleton_Duplicate_LastWins()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        // Second registration should not throw — last registration wins
        Assert.DoesNotThrow(() => collection.AddSingleton<SimpleService>());
    }

    // AddSingleton with multiple constructors is now a compile-time error (GK0002)

    [Test]
    public void AddSingleton_MissingDependency_Throws()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<ServiceWithDependency>();

        Assert.Throws<InvalidOperationException>(() => collection.BuildServiceProvider());
    }

    [Test]
    public void AddSingleton_CircularDependency_Throws()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<CircularServiceA>();
        collection.AddSingleton<CircularServiceB>();

        Assert.Throws<InvalidOperationException>(() => collection.BuildServiceProvider());
    }

    // --- AddSingleton<T>(T instance) ---

    [Test]
    public void AddSingleton_Instance_ReturnsExactInstance()
    {
        ServiceCollection collection = new();
        SimpleService instance = new();
        collection.AddSingleton(instance);

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(provider.GetService<SimpleService>(), Is.SameAs(instance));
    }

    [Test]
    public void AddSingleton_Instance_Duplicate_LastWins()
    {
        ServiceCollection collection = new();
        SimpleService first = new();
        SimpleService second = new();
        collection.AddSingleton(first);
        collection.AddSingleton(second);

        ServiceProvider provider = collection.BuildServiceProvider();

        // Second registration wins
        Assert.That(provider.GetService<SimpleService>(), Is.SameAs(second));
    }

    // --- AddSingleton<T>(Delegate) ---

    [Test]
    public void AddSingleton_Factory_ResolvesFromFactory()
    {
        ServiceCollection collection = new();
        SimpleService expected = new();
        collection.AddSingleton<SimpleService>(() => expected);

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(provider.GetService<SimpleService>(), Is.SameAs(expected));
    }

    [Test]
    public void AddSingleton_Factory_WithDependencyParameter_ResolvesDependency()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        collection.AddSingleton<ServiceWithDependency>((SimpleService s) => new ServiceWithDependency(s));

        ServiceProvider provider = collection.BuildServiceProvider();

        ServiceWithDependency service = provider.GetService<ServiceWithDependency>();

        Assert.That(service.Simple, Is.SameAs(provider.GetService<SimpleService>()));
    }

    [Test]
    public void AddSingleton_Factory_Duplicate_LastWins()
    {
        ServiceCollection collection = new();
        SimpleService first = new();
        SimpleService second = new();
        collection.AddSingleton<SimpleService>(() => first);
        collection.AddSingleton<SimpleService>(() => second);

        ServiceProvider provider = collection.BuildServiceProvider();

        // Second factory registration wins
        Assert.That(provider.GetService<SimpleService>(), Is.SameAs(second));
    }

    // --- AddSingleton<TService, TImplementation>() ---

    [Test]
    public void AddSingleton_WithAlias_ResolvesViaInterface()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<IMyService, MyServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        IMyService service = provider.GetService<IMyService>();

        Assert.That(service, Is.InstanceOf<MyServiceImpl>());
    }

    [Test]
    public void AddSingleton_WithAlias_OnlyRegistersUnderServiceType_NotImplType()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<IMyService, MyServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        // TImpl is NOT separately registered — only TService is
        Assert.Throws<InvalidOperationException>(() => provider.GetService<MyServiceImpl>());
    }

    [Test]
    public void AddSingleton_WithAlias_DuplicateServiceType_LastWins()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<IMyService, MyServiceImpl>();
        collection.AddSingleton<IMyService, AnotherServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        // Second registration wins
        Assert.That(provider.GetService<IMyService>(), Is.InstanceOf<AnotherServiceImpl>());
    }

    // --- AddAlias ---

    [Test]
    public void AddAlias_ResolvesViaInterface()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<MyServiceImpl>();
        collection.AddAlias<IMyService, MyServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        IMyService service = provider.GetService<IMyService>();

        Assert.That(service, Is.InstanceOf<MyServiceImpl>());
        Assert.That(service, Is.SameAs(provider.GetService<MyServiceImpl>()));
    }

    [Test]
    public void AddAlias_Duplicate_LastWins()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<MyServiceImpl>();
        collection.AddSingleton<AnotherServiceImpl>();
        collection.AddAlias<IMyService, MyServiceImpl>();

        // Second alias for the same TService should not throw — last wins
        Assert.DoesNotThrow(() => collection.AddAlias<IMyService, AnotherServiceImpl>());
    }

    // --- GetServices ---

    [Test]
    public void GetServices_ReturnsEmptyCollection_WhenNothingRegistered()
    {
        ServiceCollection collection = new();
        ServiceProvider provider = collection.BuildServiceProvider();

        IReadOnlyList<SimpleService> services = provider.GetServices<SimpleService>();

        Assert.That(services, Is.Empty);
    }

    [Test]
    public void GetServices_ReturnsSingleRegistration()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        IReadOnlyList<SimpleService> services = provider.GetServices<SimpleService>();

        Assert.That(services, Has.Count.EqualTo(1));
        Assert.That(services[0], Is.SameAs(provider.GetService<SimpleService>()));
    }

    [Test]
    public void GetServices_ReturnsAllRegistrationsInOrder()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<IMyService, MyServiceImpl>();
        collection.AddSingleton<IMyService, AnotherServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        IReadOnlyList<IMyService> services = provider.GetServices<IMyService>();

        Assert.That(services, Has.Count.EqualTo(2));
        Assert.That(services[0], Is.InstanceOf<MyServiceImpl>());
        Assert.That(services[1], Is.InstanceOf<AnotherServiceImpl>());
    }

    [Test]
    public void GetServices_LastRegistrationMatchesGetService()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<IMyService, MyServiceImpl>();
        collection.AddSingleton<IMyService, AnotherServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        IReadOnlyList<IMyService> services = provider.GetServices<IMyService>();

        // GetService<T> returns last-wins; GetServices returns all
        Assert.That(provider.GetService<IMyService>(), Is.SameAs(services[1]));
    }

    [Test]
    public void GetServices_AddSingletonDuplicate_CollectsAll()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        collection.AddSingleton<SimpleService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        IReadOnlyList<SimpleService> services = provider.GetServices<SimpleService>();

        Assert.That(services, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetServices_FactoryCalledOncePerRegistration()
    {
        ServiceCollection collection = new();
        List<object> activated = new();
        collection.OnActivation(obj => activated.Add(obj));

        // Register the same service type twice; both instances must go through OnActivation
        collection.AddSingleton<SimpleService>(() => new SimpleService());
        collection.AddSingleton<SimpleService>(() => new SimpleService());

        ServiceProvider provider = collection.BuildServiceProvider();
        IReadOnlyList<SimpleService> services = provider.GetServices<SimpleService>();

        Assert.That(services, Has.Count.EqualTo(2));
        // Every instance returned by GetServices must have triggered OnActivation
        Assert.That(activated, Has.Count.EqualTo(2));
        Assert.That(activated, Contains.Item(services[0]));
        Assert.That(activated, Contains.Item(services[1]));
    }

    // --- GetService / TryGetService ---

    [Test]
    public void GetService_Unregistered_Throws()
    {
        ServiceCollection collection = new();
        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetService<SimpleService>());
    }

    [Test]
    public void TryGetService_Unregistered_ReturnsNull()
    {
        ServiceCollection collection = new();
        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(provider.TryGetService<SimpleService>(), Is.Null);
    }

    [Test]
    public void TryGetService_Registered_ReturnsService()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(provider.TryGetService<SimpleService>(), Is.Not.Null);
    }

    // --- ServiceProvider self-registration ---

    [Test]
    public void ServiceProvider_IsResolvable()
    {
        ServiceCollection collection = new();

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(provider.GetService<ServiceProvider>(), Is.SameAs(provider));
    }

    [Test]
    public void ServiceProvider_InjectableViaConstructor()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<ServiceNeedingProvider>();

        ServiceProvider provider = collection.BuildServiceProvider();

        ServiceNeedingProvider service = provider.GetService<ServiceNeedingProvider>();

        Assert.That(service.Provider, Is.SameAs(provider));
    }

    // --- IsRegistered ---

    [Test]
    public void IsRegistered_ReturnsTrueForRegisteredType()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        Assert.That(collection.IsRegistered<SimpleService>(), Is.True);
        Assert.That(collection.IsRegistered(typeof(SimpleService)), Is.True);
    }

    [Test]
    public void IsRegistered_ReturnsFalseForUnregisteredType()
    {
        ServiceCollection collection = new();

        Assert.That(collection.IsRegistered<SimpleService>(), Is.False);
    }

    // --- OnActivation ---

    [Test]
    public void OnActivation_CalledForEachInstance()
    {
        ServiceCollection collection = new();
        List<object> activated = new();
        collection.OnActivation(obj => activated.Add(obj));

        collection.AddSingleton<SimpleService>();
        collection.AddSingleton<AnotherService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(activated, Has.Count.EqualTo(2));
        Assert.That(activated, Has.Some.InstanceOf<SimpleService>());
        Assert.That(activated, Has.Some.InstanceOf<AnotherService>());
    }

    [Test]
    public void OnActivation_CalledForInstanceRegistration()
    {
        ServiceCollection collection = new();
        List<object> activated = new();
        collection.OnActivation(obj => activated.Add(obj));

        SimpleService instance = new();
        collection.AddSingleton(instance);

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(activated, Has.Count.EqualTo(1));
        Assert.That(activated[0], Is.SameAs(instance));
    }

    [Test]
    public void OnActivation_CalledForFactoryRegistration()
    {
        ServiceCollection collection = new();
        List<object> activated = new();
        collection.OnActivation(obj => activated.Add(obj));

        collection.AddSingleton<SimpleService>(() => new SimpleService());

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(activated, Has.Count.EqualTo(1));
        Assert.That(activated[0], Is.InstanceOf<SimpleService>());
    }

    [Test]
    public void OnActivation_NotCalledForAlias()
    {
        ServiceCollection collection = new();
        List<object> activated = new();
        collection.OnActivation(obj => activated.Add(obj));

        collection.AddSingleton<IMyService, MyServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        // Only one activation: the concrete type, not the alias
        Assert.That(activated, Has.Count.EqualTo(1));
        Assert.That(activated[0], Is.InstanceOf<MyServiceImpl>());
    }

    // --- OnStart ---

    [Test]
    public void OnStart_CalledAfterAllResolved()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        SimpleService? captured = null;
        collection.OnStart((SimpleService s) => { captured = s; });

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(captured, Is.SameAs(provider.GetService<SimpleService>()));
    }

    [Test]
    public void OnStart_CalledInOrder()
    {
        ServiceCollection collection = new();
        List<int> order = new();

        collection.OnStart(() => { order.Add(1); });
        collection.OnStart(() => { order.Add(2); });

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(order, Is.EqualTo(new[] { 1, 2 }));
    }

    // --- OnDispose ---

    [Test]
    public void OnDispose_CalledOnDispose()
    {
        ServiceCollection collection = new();
        bool disposed = false;
        collection.OnDispose(_ => disposed = true);

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(disposed, Is.False);

        provider.Dispose();

        Assert.That(disposed, Is.True);
    }

    [Test]
    public void OnDispose_ReceivesServiceProvider()
    {
        ServiceCollection collection = new();
        ServiceProvider? received = null;
        collection.OnDispose(sp => received = sp);

        ServiceProvider provider = collection.BuildServiceProvider();
        provider.Dispose();

        Assert.That(received, Is.SameAs(provider));
    }

    [Test]
    public void Dispose_DisposesDisposableServices()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<DisposableService>();

        ServiceProvider provider = collection.BuildServiceProvider();
        DisposableService service = provider.GetService<DisposableService>();

        Assert.That(service.Disposed, Is.False);

        provider.Dispose();

        Assert.That(service.Disposed, Is.True);
    }

    // --- Subcontainers ---

    [Test]
    public void ChildContainer_ResolvesOwnServices()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton<SimpleService>();
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<AnotherService>();
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(child.GetService<AnotherService>(), Is.Not.Null);
    }

    [Test]
    public void ChildContainer_FallsBackToParent()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton<SimpleService>();
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<AnotherService>();
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(child.GetService<SimpleService>(), Is.SameAs(root.GetService<SimpleService>()));
    }

    [Test]
    public void ChildContainer_ConstructorResolvesFromParent()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton<SimpleService>();
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<ServiceWithDependency>();
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        ServiceWithDependency service = child.GetService<ServiceWithDependency>();

        Assert.That(service.Simple, Is.SameAs(root.GetService<SimpleService>()));
    }

    [Test]
    public void ChildContainer_OwnServiceProviderIsSelf()
    {
        ServiceCollection rootCollection = new();
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(child.GetService<ServiceProvider>(), Is.SameAs(child));
    }

    [Test]
    public void ChildContainer_CanOverrideParentService()
    {
        ServiceCollection rootCollection = new();
        SimpleService rootInstance = new();
        rootCollection.AddSingleton(rootInstance);
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        SimpleService childInstance = new();
        childCollection.AddSingleton(childInstance);
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(child.GetService<SimpleService>(), Is.SameAs(childInstance));
        Assert.That(root.GetService<SimpleService>(), Is.SameAs(rootInstance));
    }

    [Test]
    public void ChildContainer_TryGetService_FallsBackToParent()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton<SimpleService>();
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(child.TryGetService<SimpleService>(), Is.SameAs(root.GetService<SimpleService>()));
    }

    [Test]
    public void ChildContainer_TryGetService_ReturnsNullIfNowhere()
    {
        ServiceCollection rootCollection = new();
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(child.TryGetService<SimpleService>(), Is.Null);
    }

    // --- Registration order independence ---

    [Test]
    public void AddSingleton_DependencyRegisteredAfter_StillResolves()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<ServiceWithDependency>();
        collection.AddSingleton<SimpleService>();

        ServiceProvider provider = collection.BuildServiceProvider();

        ServiceWithDependency service = provider.GetService<ServiceWithDependency>();

        Assert.That(service.Simple, Is.SameAs(provider.GetService<SimpleService>()));
    }
    // --- Double dispose ---

    [Test]
    public void Dispose_CalledTwice_OnlyFiresCallbacksOnce()
    {
        ServiceCollection collection = new();
        int disposeCount = 0;
        collection.OnDispose(_ => disposeCount++);

        ServiceProvider provider = collection.BuildServiceProvider();
        provider.Dispose();
        provider.Dispose();

        Assert.That(disposeCount, Is.EqualTo(1));
    }

    // --- Parent-child alias ---

    [Test]
    public void ChildContainer_AliasToParentType_Resolves()
    {
        ServiceCollection rootCollection = new();
        rootCollection.AddSingleton<MyServiceImpl>();
        ServiceProvider root = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IMyService, MyServiceImpl>();
        ServiceProvider child = childCollection.BuildServiceProvider(root);

        Assert.That(child.GetService<IMyService>(), Is.InstanceOf<MyServiceImpl>());
    }

    // --- TryGetService during build ---

    [Test]
    public void TryGetService_DuringBuild_ResolvesRegisteredService()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();

        SimpleService? captured = null;
        collection.AddSingleton<AnotherService>((ServiceProvider sp) =>
        {
            captured = sp.TryGetService<SimpleService>();
            return new AnotherService();
        });

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(captured, Is.SameAs(provider.GetService<SimpleService>()));
    }

    [Test]
    public void TryGetService_DuringBuild_ReturnsNullForUnregistered()
    {
        ServiceCollection collection = new();

        SimpleService? captured = null;
        collection.AddSingleton<AnotherService>((ServiceProvider sp) =>
        {
            captured = sp.TryGetService<SimpleService>();
            return new AnotherService();
        });

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(captured, Is.Null);
    }

    // --- Factory returning null ---

    [Test]
    public void AddSingleton_Factory_ReturnsNull_Throws()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>((Func<SimpleService>)(() => null!));

        Assert.Throws<InvalidOperationException>(() => collection.BuildServiceProvider());
    }

    // --- ServiceCollection reuse ---

    [Test]
    public void BuildServiceProvider_Twice_ProducesIndependentProviders()
    {
        ServiceCollection collection = new();
        int disposeCount = 0;
        collection.OnDispose(_ => disposeCount++);
        collection.AddSingleton<SimpleService>();

        ServiceProvider first = collection.BuildServiceProvider();
        ServiceProvider second = collection.BuildServiceProvider();

        first.Dispose();

        Assert.That(disposeCount, Is.EqualTo(1));
        Assert.That(second.GetService<SimpleService>(), Is.Not.SameAs(first.GetService<SimpleService>()));
    }

    // --- Bug: Dispose order must be reverse creation order ---

    [Test]
    public void Dispose_DisposesServicesInReverseCreationOrder()
    {
        List<string> disposeLog = new();

        ServiceCollection collection = new();
        // Register A, then B, then C — creation order is A, B, C
        collection.AddSingleton<TrackingDisposableA>(new TrackingDisposableA(disposeLog));
        collection.AddSingleton<TrackingDisposableB>(new TrackingDisposableB(disposeLog));
        collection.AddSingleton<TrackingDisposableC>(new TrackingDisposableC(disposeLog));

        ServiceProvider provider = collection.BuildServiceProvider();
        provider.Dispose();

        // Expected: reverse creation order C, B, A
        Assert.That(disposeLog, Is.EqualTo(new[] { "C", "B", "A" }));
    }

    // --- Bug: GetServices called during build must not corrupt the last-wins factory slot ---

    [Test]
    public void GetServices_CalledDuringBuild_LastWinsFactoryResolvesToLastInstance()
    {
        ServiceCollection collection = new();
        MyServiceImpl firstInstance = new();
        AnotherServiceImpl lastInstance = new();

        // Registered first so it is resolved before IMyService in the main build loop,
        // ensuring GetServices<IMyService> fires while the last-wins slot is still empty.
        collection.AddSingleton<AnotherService>((ServiceProvider sp) =>
        {
            sp.GetServices<IMyService>();
            return new AnotherService();
        });

        collection.AddSingleton<IMyService>((ServiceProvider _) => firstInstance);
        collection.AddSingleton<IMyService>((ServiceProvider _) => lastInstance);

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(provider.GetService<IMyService>(), Is.SameAs(lastInstance));

        IReadOnlyList<IMyService> services = provider.GetServices<IMyService>();
        Assert.That(services, Has.Count.EqualTo(2));
        Assert.That(services[0], Is.SameAs(firstInstance));
        Assert.That(services[1], Is.SameAs(lastInstance));
    }

    // --- Bug: GetServices called during build must resolve alias sources eagerly ---

    [Test]
    public void GetServices_CalledDuringBuild_IncludesAliasSource()
    {
        ServiceCollection collection = new();
        IReadOnlyList<IMyService>? capturedDuringBuild = null;

        // Registered first so it is resolved before MyServiceImpl in the main build loop,
        // ensuring GetServices<IMyService> fires while the alias source is still unresolved.
        collection.AddSingleton<AnotherService>((ServiceProvider sp) =>
        {
            capturedDuringBuild = sp.GetServices<IMyService>();
            return new AnotherService();
        });

        collection.AddSingleton<MyServiceImpl>();
        collection.AddAlias<IMyService, MyServiceImpl>();

        ServiceProvider provider = collection.BuildServiceProvider();

        Assert.That(capturedDuringBuild, Has.Count.EqualTo(1));
        Assert.That(capturedDuringBuild![0], Is.InstanceOf<MyServiceImpl>());
    }

    // --- Bug: Aliased services must not be double-disposed ---

    [Test]
    public void Dispose_AliasedService_DisposedExactlyOnce()
    {
        CountingDisposable instance = new();

        ServiceCollection collection = new();
        collection.AddSingleton<CountingDisposable>(instance);
        collection.AddAlias<IDisposableAlias, CountingDisposable>();

        ServiceProvider provider = collection.BuildServiceProvider();
        provider.Dispose();

        // The same instance sits in two slots; Dispose() must be called exactly once
        Assert.That(instance.DisposeCallCount, Is.EqualTo(1));
    }
}

public class ServiceNeedingProvider
{
    public ServiceProvider Provider { get; }

    public ServiceNeedingProvider(ServiceProvider provider)
    {
        Provider = provider;
    }
}

public interface IDisposableAlias;

public class TrackingDisposableA : IDisposable
{
    private readonly List<string> _log;

    public TrackingDisposableA(List<string> log)
    {
        _log = log;
    }

    public void Dispose()
    {
        _log.Add("A");
    }
}

public class TrackingDisposableB : IDisposable
{
    private readonly List<string> _log;

    public TrackingDisposableB(List<string> log)
    {
        _log = log;
    }

    public void Dispose()
    {
        _log.Add("B");
    }
}

public class TrackingDisposableC : IDisposable
{
    private readonly List<string> _log;

    public TrackingDisposableC(List<string> log)
    {
        _log = log;
    }

    public void Dispose()
    {
        _log.Add("C");
    }
}

public class CountingDisposable : IDisposable, IDisposableAlias
{
    public int DisposeCallCount { get; private set; }

    public void Dispose()
    {
        DisposeCallCount++;
    }
}
