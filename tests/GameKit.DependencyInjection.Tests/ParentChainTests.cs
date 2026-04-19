using GameKit.DependencyInjection;

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

public class DoubleDisposeTracker : IDisposable
{
    public int DisposeCallCount { get; private set; }

    public void Dispose()
    {
        DisposeCallCount++;
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
    public void GetServices_UsesChildCollection_WhenChildRegistersType_DoesNotMergeParent()
    {
        ServiceCollection parentCollection = new();
        parentCollection.AddSingleton<IMyService, MyServiceImpl>();
        parentCollection.AddSingleton<IMyService, AnotherServiceImpl>();
        ServiceProvider parent = parentCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        // Child registers only one IMyService — parent's two-item collection is not merged
        childCollection.AddSingleton<IMyService, AnotherServiceImpl>();
        ServiceProvider child = childCollection.BuildServiceProvider(parent);

        IReadOnlyList<IMyService> services = child.GetServices<IMyService>();

        Assert.That(services, Has.Count.EqualTo(1));
        Assert.That(services[0], Is.InstanceOf<AnotherServiceImpl>());
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
    // 5. ServiceProvider.Empty
    // -------------------------------------------------------------------------

    [Test]
    public void Empty_GetRequiredService_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => ServiceProvider.Empty.GetRequiredService<SimpleService>());
    }

    [Test]
    public void Empty_GetService_ReturnsNull()
    {
        Assert.That(ServiceProvider.Empty.GetService<SimpleService>(), Is.Null);
    }

    [Test]
    public void Empty_GetServices_ReturnsEmptyList()
    {
        Assert.That(ServiceProvider.Empty.GetServices<SimpleService>(), Is.Empty);
    }

    [Test]
    public void Empty_Dispose_DoesNotThrow()
    {
        // Empty is a shared singleton — disposal must be a safe no-op.
        // Calling Dispose once sets _disposed, making subsequent calls no-ops too.
        Assert.DoesNotThrow(() => ServiceProvider.Empty.Dispose());
    }

    [Test]
    public void Empty_UsedAsParent_ChildResolvesOwnServices()
    {
        ServiceCollection childCollection = new();
        childCollection.AddSingleton<SimpleService>();
        ServiceProvider child = childCollection.BuildServiceProvider(ServiceProvider.Empty);

        Assert.That(child.GetRequiredService<SimpleService>(), Is.Not.Null);
    }

    [Test]
    public void Empty_UsedAsParent_MissingServiceReturnsNull()
    {
        ServiceCollection childCollection = new();
        childCollection.AddSingleton<SimpleService>();
        ServiceProvider child = childCollection.BuildServiceProvider(ServiceProvider.Empty);

        // AnotherService is neither in child nor in Empty
        Assert.That(child.GetService<AnotherService>(), Is.Null);
    }

    [Test]
    public void Empty_UsedAsParent_MissingServiceThrowsOnGetRequired()
    {
        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(ServiceProvider.Empty);

        Assert.Throws<InvalidOperationException>(
            () => child.GetRequiredService<SimpleService>());
    }

    [Test]
    public void Empty_UsedAsParent_GetServices_ReturnsEmptyForUnregisteredType()
    {
        ServiceCollection childCollection = new();
        ServiceProvider child = childCollection.BuildServiceProvider(ServiceProvider.Empty);

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
}
