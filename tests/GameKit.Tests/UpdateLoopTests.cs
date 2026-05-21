using GameKit.App;
using GameKit.DependencyInjection;

namespace GameKit.Tests;

public class UpdateLoopTests
{
    [Test]
    public void Update_UpdatesRegisteredUpdatables()
    {
        UpdateLoop updateLoop = new();
        LoopTestUpdatable updatable = new();

        updateLoop.Register(updatable);
        updateLoop.Update();

        Assert.That(updatable.UpdateCount, Is.EqualTo(1));
    }

    [Test]
    public void Register_IgnoresDuplicateInstances()
    {
        UpdateLoop updateLoop = new();
        LoopTestUpdatable updatable = new();

        updateLoop.Register(updatable);
        updateLoop.Register(updatable);

        Assert.That(updateLoop.Count, Is.EqualTo(1));
    }

    [Test]
    public void Unregister_RemovesRegisteredInstance()
    {
        UpdateLoop updateLoop = new();
        LoopTestUpdatable updatable = new();

        updateLoop.Register(updatable);
        updateLoop.Unregister(updatable);
        updateLoop.Update();

        Assert.That(updateLoop.Count, Is.EqualTo(0));
        Assert.That(updatable.UpdateCount, Is.EqualTo(0));
    }

    [Test]
    public void Update_SkipsPendingUpdatablesUnregisteredDuringFrame()
    {
        UpdateLoop updateLoop = new();
        LoopTestUpdatable first = new();
        LoopTestUpdatable second = new();
        first.OnUpdate = () => updateLoop.Unregister(second);

        updateLoop.Register(first);
        updateLoop.Register(second);
        updateLoop.Update();

        Assert.That(first.UpdateCount, Is.EqualTo(1));
        Assert.That(second.UpdateCount, Is.EqualTo(0));
        Assert.That(updateLoop.Count, Is.EqualTo(1));
    }

    [Test]
    public void RegisterUpdatables_RegistersRootAndChildUpdatables_AndUnregistersChildOnDispose()
    {
        UpdateLoop updateLoop = new();
        ServiceCollection rootCollection = new();
        rootCollection.OnActivated((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateLoop.Register(updatable);
            }
        });
        rootCollection.OnDisposing((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateLoop.Unregister(updatable);
            }
        });
        rootCollection.AddSingleton<LoopTestUpdatable>(_ => new LoopTestUpdatable());
        ServiceProvider rootProvider = rootCollection.BuildServiceProvider();

        ServiceCollection childCollection = new();
        childCollection.AddSingleton<ChildLoopTestUpdatable>(_ => new ChildLoopTestUpdatable());
        ServiceProvider childProvider = childCollection.BuildServiceProvider(rootProvider);

        Assert.That(updateLoop.Count, Is.EqualTo(2));

        childProvider.Dispose();
        updateLoop.Update();

        Assert.That(updateLoop.Count, Is.EqualTo(1));

        rootProvider.Dispose();
        updateLoop.Update();

        Assert.That(updateLoop.Count, Is.EqualTo(0));
    }

    private sealed class LoopTestUpdatable : IUpdatable
    {
        public Action? OnUpdate { get; set; }

        public int UpdateCount { get; private set; }

        public void Update()
        {
            UpdateCount++;
            OnUpdate?.Invoke();
        }
    }

    private sealed class ChildLoopTestUpdatable : IUpdatable
    {
        public void Update()
        {
        }
    }
}
