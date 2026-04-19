using GameKit;
using GameKit.Collections;
using GameKit.DependencyInjection;

namespace GameKit.Componentize.Tests;

public class DerivedTestComponent : TestComponent;

public class TickableComponent : GameComponent
{
    private readonly UpdateSystem _updateSystem;
    private Handle<UpdateTag> _tickHandle;

    public int TickCount { get; private set; }

    public TickableComponent(UpdateSystem updateSystem)
    {
        _updateSystem = updateSystem;
    }

    protected override void OnAttach()
    {
        _tickHandle = _updateSystem.Add(Tick);
    }

    protected override void OnDetach()
    {
        _updateSystem.Remove(_tickHandle);
    }

    private void Tick() => TickCount++;
}

public class CrossAttachOnDetachComponent : GameComponent
{
    public GameObject? Target { get; set; }

    protected override void OnDetach()
    {
        Target?.Attach<TestComponent>();
    }
}

public class GameWorldTests
{
    GameWorld _world;
    UpdateSystem _updateSystem;

    [SetUp]
    public void Setup()
    {
        _updateSystem = new UpdateSystem();
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<UpdateSystem>(_updateSystem);
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
        TickableComponent tickable = new TickableComponent(_updateSystem);
        gameObject.Attach(tickable);

        _updateSystem.Update();
        _updateSystem.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(2));
    }

    [Test]
    public void Update_DetachedTickableComponentStopsReceivingTicks()
    {
        GameObject gameObject = _world.CreateGameObject();
        TickableComponent tickable = new TickableComponent(_updateSystem);
        gameObject.Attach(tickable);

        _updateSystem.Update();
        gameObject.Detach(tickable);
        _updateSystem.Update();

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
    public void Resolve_ExposedComponent_ReturnsComponent()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _world.Expose<TestComponent>(component);

        TestComponent resolved = _world.Resolve<TestComponent>();

        Assert.That(resolved, Is.SameAs(component));
    }

    [Test]
    public void TryResolve_ExposedComponent_ReturnsComponent()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _world.Expose<TestComponent>(component);

        TestComponent? resolved = _world.TryResolve<TestComponent>();

        Assert.That(resolved, Is.SameAs(component));
    }

    [Test]
    public void TryResolve_NothingExposed_ReturnsNull()
    {
        TestComponent? resolved = _world.TryResolve<TestComponent>();

        Assert.That(resolved, Is.Null);
    }

    [Test]
    public void Resolve_NothingExposed_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _world.Resolve<TestComponent>());
    }

    [Test]
    public void Expose_DuplicateType_Throws()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent first = new TestComponent();
        TestComponent second = new TestComponent();
        gameObject.Attach(first);
        gameObject.Attach(second);
        _world.Expose<TestComponent>(first);

        Assert.Throws<InvalidOperationException>(() => _world.Expose<TestComponent>(second));
    }

    [Test]
    public void Revoke_ExposedComponent_RemovesFromResolution()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _world.Expose<TestComponent>(component);
        _world.Revoke<TestComponent>(component);

        Assert.That(_world.TryResolve<TestComponent>(), Is.Null);
    }

    [Test]
    public void Revoke_WrongComponent_Throws()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent exposed = new TestComponent();
        TestComponent other = new TestComponent();
        gameObject.Attach(exposed);
        gameObject.Attach(other);
        _world.Expose<TestComponent>(exposed);

        Assert.Throws<InvalidOperationException>(() => _world.Revoke<TestComponent>(other));
    }

    [Test]
    public void Revoke_NothingExposed_Throws()
    {
        TestComponent component = new TestComponent();

        Assert.Throws<InvalidOperationException>(() => _world.Revoke<TestComponent>(component));
    }

    [Test]
    public void Expose_DerivedAsBase_ResolvesAsBase()
    {
        GameObject gameObject = _world.CreateGameObject();
        DerivedTestComponent derived = new DerivedTestComponent();
        gameObject.Attach(derived);
        _world.Expose<TestComponent>(derived);

        TestComponent resolved = _world.Resolve<TestComponent>();

        Assert.That(resolved, Is.SameAs(derived));
    }

}
