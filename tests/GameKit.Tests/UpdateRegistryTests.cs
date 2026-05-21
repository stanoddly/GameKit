using GameKit.App;
using GameKit.DependencyInjection;

namespace GameKit.Tests;

public class UpdateRegistryTests
{
    [Test]
    public void Snapshot_ReturnsRegisteredUpdatables()
    {
        UpdateRegistry registry = new();
        RegistryTestUpdatable updatable = new();

        registry.Register(updatable);

        IUpdatable[] updatables = registry.Snapshot();

        Assert.That(updatables, Has.Length.EqualTo(1));
        Assert.That(updatables[0], Is.SameAs(updatable));
    }

    [Test]
    public void Register_IgnoresDuplicateInstances()
    {
        UpdateRegistry registry = new();
        RegistryTestUpdatable updatable = new();

        registry.Register(updatable);
        registry.Register(updatable);

        Assert.That(registry.Snapshot(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Unregister_RemovesRegisteredInstance()
    {
        UpdateRegistry registry = new();
        RegistryTestUpdatable updatable = new();

        registry.Register(updatable);
        registry.Unregister(updatable);

        Assert.That(registry.Snapshot(), Is.Empty);
    }

    [Test]
    public void RegisterUpdatables_RegistersRootAndChildUpdatables_AndUnregistersChildOnDispose()
    {
        ServiceCollection rootCollection = new();
        UpdateRegistry registry = new();
        rootCollection.OnActivated((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                registry.Register(updatable);
            }
        });
        rootCollection.OnDisposing((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                registry.Unregister(updatable);
            }
        });
        rootCollection.AddSingleton<RegistryTestUpdatable>(_ => new RegistryTestUpdatable());
        ServiceProvider rootProvider = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<ChildRegistryTestUpdatable>(_ => new ChildRegistryTestUpdatable());
        ServiceProvider childProvider = childCollection.BuildServiceProvider(rootProvider);

        Assert.That(registry.Snapshot(), Has.Length.EqualTo(2));

        childProvider.Dispose();

        IUpdatable[] remainingUpdatables = registry.Snapshot();
        Assert.That(remainingUpdatables, Has.Length.EqualTo(1));
        Assert.That(remainingUpdatables[0], Is.SameAs(rootProvider.GetRequiredService<RegistryTestUpdatable>()));

        rootProvider.Dispose();

        Assert.That(registry.Snapshot(), Is.Empty);
    }

    private sealed class RegistryTestUpdatable : IUpdatable
    {
        public void Update()
        {
        }
    }

    private sealed class ChildRegistryTestUpdatable : IUpdatable
    {
        public void Update()
        {
        }
    }
}
