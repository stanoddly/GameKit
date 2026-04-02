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

public class GameWorldTests
{
    GameWorld _world;

    [SetUp]
    public void Setup()
    {
        _world = new GameWorld();
    }

    [Test]
    public void OnComponentAttached_CallbackInvokedWhenComponentAttached()
    {
        GameComponent? received = null;
        GameObject? receivedOwner = null;
        _world.OnComponentAttached<TestComponent>((go, c) =>
        {
            receivedOwner = go;
            received = c;
        });

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();

        Assert.That(received, Is.Not.Null);
        Assert.That(received, Is.InstanceOf<TestComponent>());
        Assert.That(receivedOwner, Is.SameAs(gameObject));
    }

    [Test]
    public void OnComponentDetached_CallbackInvokedWhenComponentDetached()
    {
        GameComponent? received = null;
        _world.OnComponentDetached<TestComponent>((go, c) => received = c);

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();
        gameObject.Detach<TestComponent>();

        Assert.That(received, Is.Not.Null);
        Assert.That(received, Is.InstanceOf<TestComponent>());
    }

    [Test]
    public void OnComponentAttached_NotInvokedForUnrelatedType()
    {
        bool called = false;
        _world.OnComponentAttached<TestComponent2>((go, c) => called = true);

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();

        Assert.That(called, Is.False);
    }

    [Test]
    public void OnComponentAttached_SupportsInheritance()
    {
        GameComponent? received = null;
        _world.OnComponentAttached<TestComponent>((go, c) => received = c);

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<DerivedTestComponent>();

        Assert.That(received, Is.Not.Null);
        Assert.That(received, Is.InstanceOf<DerivedTestComponent>());
    }

    [Test]
    public void OnComponentDetached_SupportsInheritance()
    {
        GameComponent? received = null;
        _world.OnComponentDetached<TestComponent>((go, c) => received = c);

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<DerivedTestComponent>();
        gameObject.Detach<DerivedTestComponent>();

        Assert.That(received, Is.Not.Null);
        Assert.That(received, Is.InstanceOf<DerivedTestComponent>());
    }

    [Test]
    public void OnComponentAttached_WithInstance_CallbackInvoked()
    {
        GameComponent? received = null;
        _world.OnComponentAttached<TestComponent>((go, c) => received = c);

        GameObject gameObject = _world.CreateGameObject();
        TestComponent instance = new TestComponent { Value = "custom" };
        gameObject.Attach(instance);

        Assert.That(received, Is.SameAs(instance));
    }

    [Test]
    public void OnComponentDetached_DetachAll_CallbackInvokedForEach()
    {
        int count = 0;
        _world.OnComponentDetached<TestComponent>((go, c) => count++);

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();
        gameObject.Attach<TestComponent>();
        gameObject.DetachAll();

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void OnComponentDetached_RemoveGameObject_CallbackInvoked()
    {
        int count = 0;
        _world.OnComponentDetached<TestComponent>((go, c) => count++);

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();
        _world.RemoveGameObject(gameObject);

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void NoCallbacks_AttachDetachWorksWithoutError()
    {
        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();
        gameObject.Detach<TestComponent>();
    }

    [Test]
    public void OnComponentAttached_SubscribeToBaseGameComponent_ReceivesAll()
    {
        int count = 0;
        _world.OnComponentAttached<GameComponent>((go, c) => count++);

        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TestComponent>();
        gameObject.Attach<TestComponent2>();

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void Update_TickableComponentReceivesTickCalls()
    {
        GameObject gameObject = _world.CreateGameObject();
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);

        _world.Update();
        _world.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(2));
    }

    [Test]
    public void Update_DetachedTickableComponentStopsReceivingTicks()
    {
        GameObject gameObject = _world.CreateGameObject();
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);

        _world.Update();
        gameObject.Detach(tickable);
        _world.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(1));
    }

    [Test]
    public void Update_DetachAllMidTickSkipsSiblingTickable()
    {
        GameObject gameObject = _world.CreateGameObject();
        gameObject.Attach<TickableDetachAllComponent>();
        TickableComponent sibling = new TickableComponent();
        gameObject.Attach(sibling);

        _world.Update();

        Assert.That(sibling.TickCount, Is.EqualTo(0));
    }

    [Test]
    public void Update_DuplicateAttachDoesNotCauseDuplicateTicks()
    {
        GameObject gameObject = _world.CreateGameObject();
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);
        gameObject.Attach(tickable);

        _world.Update();

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
    public void DetachAll_WorldCallbackReEntersOriginalObject_DoesNotThrow()
    {
        GameObject objectA = _world.CreateGameObject();
        GameObject objectB = _world.CreateGameObject();

        _world.OnComponentAttached<TestComponent>((go, c) =>
        {
            if (go == objectB)
            {
                objectA.Attach<TestComponent2>();
            }
        });

        objectA.Attach(new CrossAttachOnDetachComponent { Target = objectB });
        objectA.Attach<TestComponent>();

        Assert.DoesNotThrow(() => objectA.DetachAll());
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
