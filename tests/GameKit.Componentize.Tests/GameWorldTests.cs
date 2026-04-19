using GameKit.DependencyInjection;

namespace GameKit.Componentize.Tests;

public class DerivedTestComponent : TestComponent;

public class TickableComponent : GameComponent, ITickable
{
    public int TickCount { get; private set; }
    public void Tick() => TickCount++;
}

public class TickableDetachAllComponent : GameComponent, ITickable
{
    public int TickCount { get; private set; }

    public void Tick()
    {
        TickCount++;
        Owner.DetachAll();
    }
}

public class CrossAttachOnDetachComponent : GameComponent
{
    public GameObject? Target { get; set; }

    protected override void OnDetach()
    {
        Target?.Attach<TestComponent>();
    }
}

public class TestTickRegistrar : ITickRegistrar
{
    private readonly List<Action> _ticks = new();

    public Action Register(Action tick)
    {
        _ticks.Add(tick);
        return () => _ticks.Remove(tick);
    }

    public void Update()
    {
        List<Action> snapshot = new List<Action>(_ticks);
        foreach (Action tick in snapshot)
        {
            tick();
        }
    }
}

public class GameWorldTests
{
    GameWorld _world;
    TestTickRegistrar _tickRegistrar;

    [SetUp]
    public void Setup()
    {
        _tickRegistrar = new TestTickRegistrar();
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<ITickRegistrar>(_tickRegistrar);
        _world = new GameWorld(services.BuildServiceProvider());
    }

    [Test]
    public void NoCallbacks_AttachDetachWorksWithoutError()
    {
        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();
        gameObject.Detach<TestComponent>();
    }

    [Test]
    public void Update_TickableComponentReceivesTickCalls()
    {
        GameObject gameObject = _world.CreateGameObject();
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);

        _tickRegistrar.Update();
        _tickRegistrar.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(2));
    }

    [Test]
    public void Update_DetachedTickableComponentStopsReceivingTicks()
    {
        GameObject gameObject = _world.CreateGameObject();
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);

        _tickRegistrar.Update();
        gameObject.Detach(tickable);
        _tickRegistrar.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(1));
    }

    [Test]
    public void Update_DetachAllMidTickSkipsSiblingTickable()
    {
        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TickableDetachAllComponent>();
        TickableComponent sibling = new TickableComponent();
        gameObject.Attach(sibling);

        _tickRegistrar.Update();

        Assert.That(sibling.TickCount, Is.EqualTo(0));
    }

    [Test]
    public void Update_DuplicateAttachDoesNotCauseDuplicateTicks()
    {
        GameObject gameObject = _world.CreateGameObject();
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);
        gameObject.Attach(tickable);

        _tickRegistrar.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveGameObject_RemovedEventFired()
    {
        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();

        GameObject? received = null;
        gameObject.Removed += go => received = go;

        _world.RemoveGameObject(gameObject);

        Assert.That(received, Is.SameAs(gameObject));
    }

    [Test]
    public void RemoveGameObject_RemovedEventFiredAfterDetachAll()
    {
        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();

        int componentCountDuringEvent = -1;
        gameObject.Removed += go => componentCountDuringEvent = go.Count();

        _world.RemoveGameObject(gameObject);

        Assert.That(componentCountDuringEvent, Is.EqualTo(0));
    }

    [Test]
    public void DetachAll_OnDetachAttachingToOtherGameObject_DoesNotThrow()
    {
        GameObject objectA = _world.CreateGameObject();
        GameObject objectB = _world.CreateGameObject();

        objectA.Attach(new CrossAttachOnDetachComponent { Target = objectB });
        objectA.Attach<TestComponent>();

        Assert.DoesNotThrow(() => objectA.DetachAll());
        Assert.That(objectB.TryGet<TestComponent>(), Is.Not.Null);
    }

    [Test]
    public void Get_AddedComponent_Returns()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _world.GlobalComponents.Add<TestComponent>(component);

        TestComponent resolved = _world.GlobalComponents.Get<TestComponent>();

        Assert.That(resolved, Is.SameAs(component));
    }

    [Test]
    public void TryGet_AddedComponent_Returns()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _world.GlobalComponents.Add<TestComponent>(component);

        TestComponent? resolved = _world.GlobalComponents.TryGet<TestComponent>();

        Assert.That(resolved, Is.SameAs(component));
    }

    [Test]
    public void TryGet_NothingAdded_ReturnsNull()
    {
        TestComponent? resolved = _world.GlobalComponents.TryGet<TestComponent>();

        Assert.That(resolved, Is.Null);
    }

    [Test]
    public void Get_NothingAdded_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _world.GlobalComponents.Get<TestComponent>());
    }

    [Test]
    public void Add_DuplicateType_Throws()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent first = new TestComponent();
        TestComponent second = new TestComponent();
        gameObject.Attach(first);
        gameObject.Attach(second);
        _world.GlobalComponents.Add<TestComponent>(first);

        Assert.Throws<InvalidOperationException>(() => _world.GlobalComponents.Add<TestComponent>(second));
    }

    [Test]
    public void Remove_AddedComponent_RemovesFromRegistry()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _world.GlobalComponents.Add<TestComponent>(component);
        _world.GlobalComponents.Remove<TestComponent>(component);

        Assert.That(_world.GlobalComponents.TryGet<TestComponent>(), Is.Null);
    }

    [Test]
    public void Remove_WrongComponent_Throws()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent registered = new TestComponent();
        TestComponent other = new TestComponent();
        gameObject.Attach(registered);
        gameObject.Attach(other);
        _world.GlobalComponents.Add<TestComponent>(registered);

        Assert.Throws<InvalidOperationException>(() => _world.GlobalComponents.Remove<TestComponent>(other));
    }

    [Test]
    public void Remove_NothingAdded_Throws()
    {
        TestComponent component = new TestComponent();

        Assert.Throws<InvalidOperationException>(() => _world.GlobalComponents.Remove<TestComponent>(component));
    }

    [Test]
    public void Add_DerivedAsBase_GetReturnsAsBase()
    {
        GameObject gameObject = _world.CreateGameObject();
        DerivedTestComponent derived = new DerivedTestComponent();
        gameObject.Attach(derived);
        _world.GlobalComponents.Add<TestComponent>(derived);

        TestComponent resolved = _world.GlobalComponents.Get<TestComponent>();

        Assert.That(resolved, Is.SameAs(derived));
    }

}
