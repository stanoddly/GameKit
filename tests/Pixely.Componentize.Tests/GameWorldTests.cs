using Pixely;
using Pixely.Collections;
using Pixely.DependencyInjection;

namespace Pixely.Componentize.Tests;

public class DerivedTestComponent : TestComponent;

public class TickableComponent : ComponentBase
{
    private readonly UpdateSystem _updateSystem;
    private Handle<UpdateTag> _tickHandle;

    public int TickCount { get; private set; }

    public TickableComponent(UpdateSystem updateSystem)
    {
        _updateSystem = updateSystem;
    }

    protected override void OnAttach(GameObject owner, ServiceProvider services)
    {
        _tickHandle = _updateSystem.Add(Tick);
    }

    protected override void OnDetach(GameObject owner, ServiceProvider services)
    {
        _updateSystem.Remove(_tickHandle);
    }

    private void Tick() => TickCount++;
}

public class CrossAttachOnDetachComponent : ComponentBase
{
    public GameObject? Target { get; set; }

    protected override void OnDetach(GameObject owner, ServiceProvider services)
    {
        Target?.Attach<TestComponent>();
    }
}

public class SiblingObservingDetachComponent : GameComponent
{
    public TestComponent? SiblingSeenDuringDetach { get; private set; }

    protected override void OnDetach()
    {
        SiblingSeenDuringDetach = TryGetSibling<TestComponent>();
    }
}

public class SelfAttachDuringDetachComponent : GameComponent
{
    public Exception? Caught { get; private set; }

    protected override void OnDetach()
    {
        try
        {
            Owner.Attach<TestComponent2>();
        }
        catch (Exception ex)
        {
            Caught = ex;
        }
    }
}

public class UncaughtSelfAttachDuringDetachComponent : GameComponent
{
    protected override void OnDetach()
    {
        Owner.Attach<TestComponent2>();
    }
}

public class ThrowingDetachComponent : GameComponent
{
    private readonly Exception _exception;

    public bool OnDetachCalled { get; private set; }

    public ThrowingDetachComponent(Exception exception)
    {
        _exception = exception;
    }

    protected override void OnDetach()
    {
        OnDetachCalled = true;
        throw _exception;
    }
}

public class GameWorldTests
{
    GameWorld _world;
    UpdateSystem _updateSystem;
    GlobalComponentRegistry _globalComponents;

    [SetUp]
    public void Setup()
    {
        _updateSystem = new UpdateSystem();
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<UpdateSystem>(_updateSystem);
        services.AddSingleton<GlobalComponentRegistry>(_ => new GlobalComponentRegistry());
        services.AddSingleton<GameWorld>(sp => new GameWorld(sp));
        ServiceProvider provider = services.BuildServiceProvider();
        _world = provider.GetRequiredService<GameWorld>();
        _globalComponents = provider.GetRequiredService<GlobalComponentRegistry>();
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
    public void RemoveGameObject_RemovedEventFiredAfterComponentsDetached()
    {
        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();

        int componentCountDuringEvent = -1;
        gameObject.Removed += go => componentCountDuringEvent = go.Count();

        _world.RemoveGameObject(gameObject);

        Assert.That(componentCountDuringEvent, Is.EqualTo(0));
    }

    [Test]
    public void RemoveGameObject_OnDetachAttachingToOtherGameObject_DoesNotThrow()
    {
        GameObject objectA = _world.CreateGameObject();
        GameObject objectB = _world.CreateGameObject();

        objectA.Attach(new CrossAttachOnDetachComponent { Target = objectB });
        objectA.Attach<TestComponent>();

        Assert.DoesNotThrow(() => _world.RemoveGameObject(objectA));
        Assert.That(objectB.TryGet<TestComponent>(), Is.Not.Null);
    }

    [Test]
    public void RemoveGameObject_SiblingsVisibleDuringOnDetach()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent sibling = gameObject.Attach<TestComponent>();
        SiblingObservingDetachComponent observer = gameObject.Attach<SiblingObservingDetachComponent>();

        _world.RemoveGameObject(gameObject);

        Assert.That(observer.SiblingSeenDuringDetach, Is.SameAs(sibling));
    }

    [Test]
    public void RemoveGameObject_AttachOnSameOwnerDuringOnDetach_Throws()
    {
        GameObject gameObject = _world.CreateGameObject();
        SelfAttachDuringDetachComponent component = gameObject.Attach<SelfAttachDuringDetachComponent>();

        _world.RemoveGameObject(gameObject);

        Assert.That(component.Caught, Is.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void RemoveGameObject_UncaughtOnDetachException_StillMarksGameObjectRemoved()
    {
        GameObject gameObject = _world.CreateGameObject();
        UncaughtSelfAttachDuringDetachComponent component = gameObject.Attach<UncaughtSelfAttachDuringDetachComponent>();
        bool removedEventFired = false;
        gameObject.Removed += _ => removedEventFired = true;

        Assert.Throws<InvalidOperationException>(() => _world.RemoveGameObject(gameObject));

        Assert.That(gameObject.State, Is.EqualTo(GameObjectState.Removed));
        Assert.That(gameObject.Count(), Is.EqualTo(0));
        Assert.That(component.HasOwner(), Is.False);
        Assert.That(removedEventFired, Is.True);
    }

    [Test]
    public void RemoveGameObject_OnDetachException_ContinuesDetachingRemainingComponents()
    {
        GameObject gameObject = _world.CreateGameObject();
        ThrowingDetachComponent first = gameObject.Attach(new ThrowingDetachComponent(new InvalidOperationException()));
        ThrowingDetachComponent second = gameObject.Attach(new ThrowingDetachComponent(new NotSupportedException()));

        AggregateException? exception = Assert.Throws<AggregateException>(() => _world.RemoveGameObject(gameObject));

        Assert.That(first.OnDetachCalled, Is.True);
        Assert.That(second.OnDetachCalled, Is.True);
        Assert.That(first.HasOwner(), Is.False);
        Assert.That(second.HasOwner(), Is.False);
        Assert.That(gameObject.State, Is.EqualTo(GameObjectState.Removed));
        Assert.That(gameObject.Count(), Is.EqualTo(0));
        Assert.That(exception?.InnerExceptions, Has.Count.EqualTo(2));
    }

    [Test]
    public void Get_AddedComponent_Returns()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _globalComponents.Add<TestComponent>(component);

        TestComponent resolved = _globalComponents.Get<TestComponent>();

        Assert.That(resolved, Is.SameAs(component));
    }

    [Test]
    public void TryGet_AddedComponent_Returns()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _globalComponents.Add<TestComponent>(component);

        TestComponent? resolved = _globalComponents.TryGet<TestComponent>();

        Assert.That(resolved, Is.SameAs(component));
    }

    [Test]
    public void TryGet_NothingAdded_ReturnsNull()
    {
        TestComponent? resolved = _globalComponents.TryGet<TestComponent>();

        Assert.That(resolved, Is.Null);
    }

    [Test]
    public void Get_NothingAdded_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _globalComponents.Get<TestComponent>());
    }

    [Test]
    public void Add_DuplicateType_Throws()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent first = new TestComponent();
        TestComponent second = new TestComponent();
        gameObject.Attach(first);
        gameObject.Attach(second);
        _globalComponents.Add<TestComponent>(first);

        Assert.Throws<InvalidOperationException>(() => _globalComponents.Add<TestComponent>(second));
    }

    [Test]
    public void Remove_AddedComponent_RemovesFromRegistry()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent component = new TestComponent();
        gameObject.Attach(component);
        _globalComponents.Add<TestComponent>(component);
        _globalComponents.Remove<TestComponent>(component);

        Assert.That(_globalComponents.TryGet<TestComponent>(), Is.Null);
    }

    [Test]
    public void Remove_WrongComponent_Throws()
    {
        GameObject gameObject = _world.CreateGameObject();
        TestComponent registered = new TestComponent();
        TestComponent other = new TestComponent();
        gameObject.Attach(registered);
        gameObject.Attach(other);
        _globalComponents.Add<TestComponent>(registered);

        Assert.Throws<InvalidOperationException>(() => _globalComponents.Remove<TestComponent>(other));
    }

    [Test]
    public void Remove_NothingAdded_Throws()
    {
        TestComponent component = new TestComponent();

        Assert.Throws<InvalidOperationException>(() => _globalComponents.Remove<TestComponent>(component));
    }

    [Test]
    public void Add_DerivedAsBase_GetReturnsAsBase()
    {
        GameObject gameObject = _world.CreateGameObject();
        DerivedTestComponent derived = new DerivedTestComponent();
        gameObject.Attach(derived);
        _globalComponents.Add<TestComponent>(derived);

        TestComponent resolved = _globalComponents.Get<TestComponent>();

        Assert.That(resolved, Is.SameAs(derived));
    }

}
