using GameKit.DependencyInjection;
using System.Reflection;

namespace GameKit.DependencyInjection.Tests;

// Services declared in ServiceCollectionTests.cs are in the same namespace and reused below:
// SimpleService, AnotherService, IMyService, MyServiceImpl, AnotherServiceImpl, DisposableService.

// Unique to this file.
public class GrandparentService;

public class ParentOnlyService;

public class ChildOnlyService;

public class ParentChainDisposable : IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
    }
}

public class ChildChainDisposable : IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose()
    {
        Disposed = true;
    }
}

public class ParentChainCallbackService;

public class ParentChainCallbackDisposable;

public class DoubleDisposeTracker : IDisposable
{
    public int DisposeCallCount { get; private set; }

    public void Dispose()
    {
        DisposeCallCount++;
    }
}

public class OrderedDisposable : IDisposable
{
    private readonly List<string> _disposeOrder;
    private readonly string _name;

    public OrderedDisposable(List<string> disposeOrder, string name)
    {
        _disposeOrder = disposeOrder;
        _name = name;
    }

    public int DisposeCallCount { get; private set; }

    public void Dispose()
    {
        DisposeCallCount++;
        _disposeOrder.Add(_name);
    }
}

public interface IChainAlias;

public class ChainAliasImpl : IChainAlias;

public class DoubleDisposeAliasTarget : IChainAlias, IDisposable
{
    public int DisposeCallCount { get; private set; }

    public void Dispose()
    {
        DisposeCallCount++;
    }
}

public class ParentChainTests
{
    // -------------------------------------------------------------------------
    // 1. Multi-level chain: grandparent → parent → child
    // -------------------------------------------------------------------------

    [Test]
    public void GetRequiredService_WalksChainToGrandparent()
    {
        ServiceCollection grandparentCollection = new();
        grandparentCollection.AddSingleton<GrandparentService>();
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        parentCollection.AddSingleton<ParentOnlyService>();
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<ChildOnlyService>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        // Child resolves its own service
        Assert.That(child.GetRequiredService<ChildOnlyService>(), Is.Not.Null);
        // Child falls through to parent
        Assert.That(child.GetRequiredService<ParentOnlyService>(), Is.SameAs(parent.GetRequiredService<ParentOnlyService>()));
        // Child falls all the way through to grandparent
        Assert.That(child.GetRequiredService<GrandparentService>(), Is.SameAs(grandparent.GetRequiredService<GrandparentService>()));
    }

    [Test]
    public void GetService_WalksChainToGrandparent()
    {
        ServiceCollection grandparentCollection = new();
        grandparentCollection.AddSingleton<GrandparentService>();
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        Assert.That(child.GetService<GrandparentService>(), Is.SameAs(grandparent.GetRequiredService<GrandparentService>()));
    }

    [Test]
    public void GetService_ReturnsNull_WhenAbsentFromEntireChain()
    {
        ServiceCollection grandparentCollection = new();
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        // SimpleService is absent from every provider in the chain
        Assert.That(child.GetService<SimpleService>(), Is.Null);
    }

    [Test]
    public void GetRequiredService_ChildWinsOverGrandparent()
    {
        ServiceCollection grandparentCollection = new();
        SimpleService grandparentInstance = new();
        grandparentCollection.AddSingleton(grandparentInstance);
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        SimpleService childInstance = new();
        childCollection.AddSingleton(childInstance);
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        Assert.That(child.GetRequiredService<SimpleService>(), Is.SameAs(childInstance));
        Assert.That(grandparent.GetRequiredService<SimpleService>(), Is.SameAs(grandparentInstance));
    }

    [Test]
    public void GetRequiredService_ParentWinsOverGrandparent_WhenChildAbsent()
    {
        ServiceCollection grandparentCollection = new();
        SimpleService grandparentInstance = new();
        grandparentCollection.AddSingleton(grandparentInstance);
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        SimpleService parentInstance = new();
        parentCollection.AddSingleton(parentInstance);
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        // Child has no SimpleService, so parent's instance takes precedence over grandparent's
        Assert.That(child.GetRequiredService<SimpleService>(), Is.SameAs(parentInstance));
    }

    // -------------------------------------------------------------------------
    // 2. GetServices multi-level chain
    // -------------------------------------------------------------------------

    [Test]
    public void GetServices_WalksChainToGrandparent_WhenNeitherChildNorParentRegistersType()
    {
        ServiceCollection grandparentCollection = new();
        grandparentCollection.AddSingleton<IMyService, MyServiceImpl>();
        grandparentCollection.AddSingleton<IMyService, AnotherServiceImpl>();
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        IReadOnlyList<IMyService> services = child.GetServices<IMyService>();

        Assert.That(services, Has.Count.EqualTo(2));
        Assert.That(services[0], Is.InstanceOf<MyServiceImpl>());
        Assert.That(services[1], Is.InstanceOf<AnotherServiceImpl>());
    }

    [Test]
    public void GetServices_ReturnsParentThenChildRegistrations_WhenChildRegistersType()
    {
        ServiceCollection parentCollection = new();
        parentCollection.AddSingleton<IMyService, MyServiceImpl>();
        parentCollection.AddSingleton<IMyService, AnotherServiceImpl>();
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<IMyService, AnotherServiceImpl>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        IReadOnlyList<IMyService> services = child.GetServices<IMyService>();

        Assert.That(services, Has.Count.EqualTo(3));
        Assert.That(services[0], Is.InstanceOf<MyServiceImpl>());
        Assert.That(services[1], Is.InstanceOf<AnotherServiceImpl>());
        Assert.That(services[2], Is.InstanceOf<AnotherServiceImpl>());
    }

    [Test]
    public void GetServices_ParentCollectionReturnedDirectly_SameReferenceAcrossChildCalls()
    {
        // Verifies zero-alloc guarantee: parent's T[] is returned without copy when
        // child has no collection for that type.
        ServiceCollection parentCollection = new();
        parentCollection.AddSingleton<IMyService, MyServiceImpl>();
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<SimpleService>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        IReadOnlyList<IMyService> first = child.GetServices<IMyService>();
        IReadOnlyList<IMyService> second = child.GetServices<IMyService>();

        // Both calls must return the exact same object — the parent's pre-built T[]
        Assert.That(second, Is.SameAs(first));
    }

    // -------------------------------------------------------------------------
    // 3. Disposal: parent-child ownership isolation
    // -------------------------------------------------------------------------

    [Test]
    public void Dispose_Child_DoesNotDisposeServicesOwnedByParent()
    {
        ServiceCollection parentCollection = new();
        ParentChainDisposable parentService = new();
        parentCollection.AddSingleton(parentService);
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<SimpleService>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        child.Dispose();

        // Parent's service must not have been touched by child's Dispose
        Assert.That(parentService.Disposed, Is.False);
    }

    [Test]
    public void Dispose_Child_DisposesOnlyChildOwnedServices()
    {
        ServiceCollection parentCollection = new();
        ParentChainDisposable parentService = new();
        parentCollection.AddSingleton(parentService);
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        ChildChainDisposable childService = new();
        childCollection.AddSingleton(childService);
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        child.Dispose();

        Assert.That(childService.Disposed, Is.True);
        Assert.That(parentService.Disposed, Is.False);
    }

    [Test]
    public void Dispose_Parent_DisposesItsOwnServices_WhileChildIsAlive()
    {
        ServiceCollection parentCollection = new();
        ParentChainDisposable parentService = new();
        parentCollection.AddSingleton(parentService);
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<SimpleService>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        // Dispose parent while child still exists
        parent.Dispose();

        Assert.That(parentService.Disposed, Is.True);
    }

    [Test]
    public void Dispose_Parent_DisposesChildrenBeforeOwnServices()
    {
        List<string> disposeOrder = new();

        ServiceCollection parentCollection = new();
        parentCollection.AddSingleton(new OrderedDisposable(disposeOrder, "parent"));
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection firstChildCollection = new();
        firstChildCollection.AddSingleton(new OrderedDisposable(disposeOrder, "firstChild"));
        ServiceProvider firstChild = firstChildCollection.BuildServiceProvider(parent);

        ServiceCollection secondChildCollection = new();
        secondChildCollection.AddSingleton(new OrderedDisposable(disposeOrder, "secondChild"));
        ServiceProvider secondChild = secondChildCollection.BuildServiceProvider(parent);

        parent.Dispose();

        Assert.Throws<ObjectDisposedException>(() => firstChild.GetRequiredService<OrderedDisposable>());
        Assert.Throws<ObjectDisposedException>(() => secondChild.GetRequiredService<OrderedDisposable>());
        Assert.That(disposeOrder, Is.EqualTo(new[] { "secondChild", "firstChild", "parent" }));
    }

    [Test]
    public void Dispose_Grandparent_DisposesDescendantsBeforeAncestors()
    {
        List<string> disposeOrder = new();

        ServiceCollection grandparentCollection = new();
        grandparentCollection.AddSingleton(new OrderedDisposable(disposeOrder, "grandparent"));
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        parentCollection.AddSingleton(new OrderedDisposable(disposeOrder, "parent"));
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        childCollection.AddSingleton(new OrderedDisposable(disposeOrder, "child"));
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        grandparent.Dispose();

        Assert.Throws<ObjectDisposedException>(() => parent.GetRequiredService<OrderedDisposable>());
        Assert.Throws<ObjectDisposedException>(() => child.GetRequiredService<OrderedDisposable>());
        Assert.That(disposeOrder, Is.EqualTo(new[] { "child", "parent", "grandparent" }));
    }

    [Test]
    public void Dispose_Child_DetachesFromParent()
    {
        List<string> disposeOrder = new();

        ServiceCollection parentCollection = new();
        parentCollection.AddSingleton(new OrderedDisposable(disposeOrder, "parent"));
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        OrderedDisposable childService = new(disposeOrder, "child");
        childCollection.AddSingleton(childService);
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        child.Dispose();
        parent.Dispose();

        Assert.That(childService.DisposeCallCount, Is.EqualTo(1));
        Assert.That(disposeOrder, Is.EqualTo(new[] { "child", "parent" }));
    }

    [Test]
    public void GetRequiredService_AfterDispose_ThrowsObjectDisposedException()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        ServiceProvider provider = collection.BuildServiceProvider();

        provider.Dispose();

        Assert.Throws<ObjectDisposedException>(() => provider.GetRequiredService<SimpleService>());
    }

    [Test]
    public void GetService_AfterDispose_ThrowsObjectDisposedException()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        ServiceProvider provider = collection.BuildServiceProvider();

        provider.Dispose();

        Assert.Throws<ObjectDisposedException>(() => provider.GetService<SimpleService>());
    }

    [Test]
    public void GetServices_AfterDispose_ThrowsObjectDisposedException()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        ServiceProvider provider = collection.BuildServiceProvider();

        provider.Dispose();

        Assert.Throws<ObjectDisposedException>(() => provider.GetServices<SimpleService>());
    }

    [Test]
    public void Dispose_ClearsServiceReferences()
    {
        ServiceCollection collection = new();
        collection.AddSingleton<SimpleService>();
        ServiceProvider provider = collection.BuildServiceProvider();

        provider.Dispose();

        Assert.That(GetPrivateField<object?>(provider, "_services"), Is.Null);
        Assert.That(GetPrivateField<object?>(provider, "_pending"), Is.Null);
        Assert.That(GetPrivateField<object?>(provider, "_serviceCollections"), Is.Null);
    }

    // -------------------------------------------------------------------------
    // 4. Double-dispose of a provider with a parent
    // -------------------------------------------------------------------------

    [Test]
    public void Dispose_ChildCalledTwice_ChildServiceDisposedExactlyOnce()
    {
        ServiceCollection parentCollection = new();
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        DoubleDisposeTracker childService = new();
        childCollection.AddSingleton(childService);
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        child.Dispose();
        child.Dispose();

        Assert.That(childService.DisposeCallCount, Is.EqualTo(1));
    }

    // -------------------------------------------------------------------------
    // 5. Empty parent provider
    // -------------------------------------------------------------------------

    [Test]
    public void EmptyProvider_UsedAsParent_ChildResolvesOwnServices()
    {
        ServiceProvider emptyParent = new ServiceCollection().BuildServiceProvider();
        ServiceCollection childCollection = new();
        childCollection.AddSingleton<SimpleService>();
        ServiceProvider child = childCollection.BuildServiceProvider(emptyParent);

        Assert.That(child.GetRequiredService<SimpleService>(), Is.Not.Null);
    }

    [Test]
    public void EmptyProvider_UsedAsParent_MissingServiceReturnsNull()
    {
        ServiceProvider emptyParent = new ServiceCollection().BuildServiceProvider();
        ServiceCollection childCollection = new();
        childCollection.AddSingleton<SimpleService>();
        ServiceProvider child = childCollection.BuildServiceProvider(emptyParent);

        // AnotherService is neither in child nor in Empty
        Assert.That(child.GetService<AnotherService>(), Is.Null);
    }

    [Test]
    public void EmptyProvider_UsedAsParent_MissingServiceThrowsOnGetRequired()
    {
        ServiceProvider emptyParent = new ServiceCollection().BuildServiceProvider();
        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(emptyParent);

        Assert.Throws<InvalidOperationException>(
            () => child.GetRequiredService<SimpleService>());
    }

    [Test]
    public void EmptyProvider_UsedAsParent_GetServices_ReturnsEmptyForUnregisteredType()
    {
        ServiceProvider emptyParent = new ServiceCollection().BuildServiceProvider();
        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(emptyParent);

        Assert.That(child.GetServices<IMyService>(), Is.Empty);
    }

    // -------------------------------------------------------------------------
    // 6. Alias in child pointing to concrete in same child collection
    //    (verifies alias + parent-chain resolution compose correctly)
    // -------------------------------------------------------------------------

    [Test]
    public void AddAlias_InChild_SourceAlsoInChild_ResolvesCorrectly()
    {
        // Both the concrete and the alias are registered in the child.
        // A parent exists but is not consulted because child owns both registrations.
        ServiceCollection parentCollection = new();
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        ChainAliasImpl concreteInstance = new();
        childCollection.AddSingleton(concreteInstance);
        childCollection.AddAlias<IChainAlias, ChainAliasImpl>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        IChainAlias alias = child.GetRequiredService<IChainAlias>();
        ChainAliasImpl concrete = child.GetRequiredService<ChainAliasImpl>();

        // Alias and concrete must be the same instance
        Assert.That(alias, Is.SameAs(concrete));
    }

    [Test]
    public void AddAlias_InChild_AliasedServiceDisposedExactlyOnce_WhenBothSlotsInChild()
    {
        ServiceCollection childCollection = new();
        DoubleDisposeAliasTarget instance = new();
        childCollection.AddSingleton(instance);
        childCollection.AddAlias<IChainAlias, DoubleDisposeAliasTarget>();
        ServiceProvider child = childCollection.BuildServiceProvider();

        child.Dispose();

        // The same instance occupies two slots in the child; must be disposed only once
        Assert.That(instance.DisposeCallCount, Is.EqualTo(1));
    }

    [Test]
    public void GetRequiredService_MultiLevelChain_AliasInGrandparent_ResolvableFromChild()
    {
        // IMyService is registered in the grandparent (via AddSingleton<IMyService, MyServiceImpl>).
        // Child and parent do not register it. Child should still resolve it via chain.
        ServiceCollection grandparentCollection = new();
        grandparentCollection.AddSingleton<IMyService, MyServiceImpl>();
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        IMyService resolved = child.GetRequiredService<IMyService>();

        Assert.That(resolved, Is.SameAs(grandparent.GetRequiredService<IMyService>()));
    }

    [Test]
    public void OnActivated_ParentCallbacksFireForChildOwnedServices()
    {
        List<string> activations = new();

        ServiceCollection parentCollection = new();
        parentCollection.OnActivated((_, type) => activations.Add($"parent:{type.Name}"));
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<ParentChainCallbackService>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        Assert.That(child.GetRequiredService<ParentChainCallbackService>(), Is.Not.Null);
        Assert.That(activations, Is.EqualTo(new[] { $"parent:{nameof(ParentChainCallbackService)}" }));
    }

    [Test]
    public void OnActivated_InheritedCallbacksRunFromAncestorToChild()
    {
        List<string> activations = new();

        ServiceCollection grandparentCollection = new();
        grandparentCollection.OnActivated((_, _) => activations.Add("grandparent"));
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        parentCollection.OnActivated((_, _) => activations.Add("parent"));
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        childCollection.OnActivated((_, _) => activations.Add("child"));
        childCollection.AddSingleton<ParentChainCallbackService>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        Assert.That(child.GetRequiredService<ParentChainCallbackService>(), Is.Not.Null);
        Assert.That(activations, Is.EqualTo(new[] { "grandparent", "parent", "child" }));
    }

    [Test]
    public void OnDisposing_InheritedCallbacksRunFromChildToAncestor()
    {
        List<string> callbacks = new();

        ServiceCollection grandparentCollection = new();
        grandparentCollection.OnDisposing((_, _) => callbacks.Add("grandparent"));
        ServiceProvider grandparent = grandparentCollection.BuildServiceProvider();

        ServiceCollection parentCollection = new();
        parentCollection.OnDisposing((_, _) => callbacks.Add("parent"));
        ServiceProvider parent = parentCollection.BuildServiceProvider(grandparent);

        ServiceCollection childCollection = new();
        childCollection.OnDisposing((_, _) => callbacks.Add("child"));
        childCollection.AddSingleton<ParentChainCallbackDisposable>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        child.Dispose();

        Assert.That(callbacks, Is.EqualTo(new[] { "child", "parent", "grandparent" }));
    }

    private static T? GetPrivateField<T>(ServiceProvider provider, string fieldName)
    {
        FieldInfo field = typeof(ServiceProvider).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field {fieldName} was not found.");
        return (T?)field.GetValue(provider);
    }
}
