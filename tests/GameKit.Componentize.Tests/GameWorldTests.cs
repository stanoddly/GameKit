using GameKit.Collections;

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

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
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

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
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

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<TestComponent>();

        Assert.That(called, Is.False);
    }

    [Test]
    public void OnComponentAttached_SupportsInheritance()
    {
        GameComponent? received = null;
        _world.OnComponentAttached<TestComponent>((go, c) => received = c);

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<DerivedTestComponent>();

        Assert.That(received, Is.Not.Null);
        Assert.That(received, Is.InstanceOf<DerivedTestComponent>());
    }

    [Test]
    public void OnComponentDetached_SupportsInheritance()
    {
        GameComponent? received = null;
        _world.OnComponentDetached<TestComponent>((go, c) => received = c);

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
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

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        TestComponent instance = new TestComponent { Value = "custom" };
        gameObject.Attach(instance);

        Assert.That(received, Is.SameAs(instance));
    }

    [Test]
    public void OnComponentDetached_DetachAll_CallbackInvokedForEach()
    {
        int count = 0;
        _world.OnComponentDetached<TestComponent>((go, c) => count++);

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
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

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<TestComponent>();
        _world.RemoveGameObject(handle);

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void NoCallbacks_AttachDetachWorksWithoutError()
    {
        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<TestComponent>();
        gameObject.Detach<TestComponent>();
    }

    [Test]
    public void OnComponentAttached_SubscribeToBaseGameComponent_ReceivesAll()
    {
        int count = 0;
        _world.OnComponentAttached<GameComponent>((go, c) => count++);

        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<TestComponent>();
        gameObject.Attach<TestComponent2>();

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void Update_TickableComponentReceivesTickCalls()
    {
        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);

        _world.Update();
        _world.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(2));
    }

    [Test]
    public void Update_DetachedTickableComponentStopsReceivingTicks()
    {
        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
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
        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<TickableDetachAllComponent>();
        TickableComponent sibling = new TickableComponent();
        gameObject.Attach(sibling);

        _world.Update();

        Assert.That(sibling.TickCount, Is.EqualTo(0));
    }

    [Test]
    public void Update_DuplicateAttachDoesNotCauseDuplicateTicks()
    {
        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        TickableComponent tickable = new TickableComponent();
        gameObject.Attach(tickable);
        gameObject.Attach(tickable);

        _world.Update();

        Assert.That(tickable.TickCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveGameObject_RemovedEventFired()
    {
        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<TestComponent>();

        GameObject? received = null;
        gameObject.Removed += go => received = go;

        _world.RemoveGameObject(handle);

        Assert.That(received, Is.SameAs(gameObject));
    }

    [Test]
    public void RemoveGameObject_RemovedEventFiredAfterDetachAll()
    {
        Handle<GameObject> handle = _world.CreateGameObject();
        GameObject gameObject = _world.GetGameObject(handle)!;
        gameObject.Attach<TestComponent>();

        int componentCountDuringEvent = -1;
        gameObject.Removed += go => componentCountDuringEvent = go.Count();

        _world.RemoveGameObject(handle);

        Assert.That(componentCountDuringEvent, Is.EqualTo(0));
    }

    [Test]
    public void DetachAll_OnDetachAttachingToOtherGameObject_DoesNotThrow()
    {
        Handle<GameObject> handleA = _world.CreateGameObject();
        GameObject objectA = _world.GetGameObject(handleA)!;
        Handle<GameObject> handleB = _world.CreateGameObject();
        GameObject objectB = _world.GetGameObject(handleB)!;

        objectA.Attach(new CrossAttachOnDetachComponent { Target = objectB });
        objectA.Attach<TestComponent>();

        Assert.DoesNotThrow(() => objectA.DetachAll());
        Assert.That(objectB.TryGet<TestComponent>(), Is.Not.Null);
    }

    [Test]
    public void DetachAll_WorldCallbackReEntersOriginalObject_DoesNotThrow()
    {
        Handle<GameObject> handleA = _world.CreateGameObject();
        GameObject objectA = _world.GetGameObject(handleA)!;
        Handle<GameObject> handleB = _world.CreateGameObject();
        GameObject objectB = _world.GetGameObject(handleB)!;

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

}
