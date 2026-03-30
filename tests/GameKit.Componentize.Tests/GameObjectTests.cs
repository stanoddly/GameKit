namespace GameKit.Componentize.Tests;

public class TestComponent : GameComponent
{
    public string Value { get; set; } = "test";
    public bool OnAttachCalled { get; private set; }
    public bool OnReadyCalled { get; private set; }
    public bool OnDetachCalled { get; private set; }

    protected override void OnAttach()
    {
        OnAttachCalled = true;
    }

    protected override void OnReady()
    {
        OnReadyCalled = true;
    }

    protected override void OnDetach()
    {
        OnDetachCalled = true;
    }
}

public class TestComponent2 : GameComponent
{
    public int Number { get; set; } = 42;
}

public class StateCapturingComponent : GameComponent
{
    public Action<StateCapturingComponent>? CaptureAction { get; set; }

    protected override void OnDetach()
    {
        CaptureAction?.Invoke(this);
    }
}

public class GameObjectTests
{
    GameWorld _world;
    GameObject _gameObject;

    [SetUp]
    public void Setup()
    {
        _world = new GameWorld();
        _gameObject = _world.CreateGameObject();
    }

    [Test]
    public void Attach_WithNewComponent_AttachesComponent()
    {
        _gameObject.Attach<TestComponent>();

        TestComponent component = _gameObject.Get<TestComponent>();
        Assert.That(component, Is.Not.Null);
        Assert.That(component.Value, Is.EqualTo("test"));
    }

    [Test]
    public void Attach_WithNewComponent_CallsOnAttach()
    {
        _gameObject.Attach<TestComponent>();

        TestComponent component = _gameObject.Get<TestComponent>();
        Assert.That(component.OnAttachCalled, Is.True);
    }

    [Test]
    public void Attach_WithNewComponent_SetsOwner()
    {
        _gameObject.Attach<TestComponent>();

        TestComponent component = _gameObject.Get<TestComponent>();
        Assert.That(component.HasOwner(), Is.True);
    }

    [Test]
    public void Attach_WithExistingComponent_AttachesNewInstance()
    {
        _gameObject.Attach<TestComponent>();
        TestComponent original = _gameObject.Get<TestComponent>();
        original.Value = "modified";

        _gameObject.Attach<TestComponent>();

        var components = _gameObject.GetComponents<TestComponent>();
        Assert.That(components.Count, Is.EqualTo(2));
        Assert.That(components[0].Value, Is.EqualTo("modified"));
        Assert.That(components[1].Value, Is.EqualTo("test"));
    }


    [Test]
    public void Attach_ReturnsComponent()
    {
        TestComponent result = _gameObject.Attach<TestComponent>();

        Assert.That(result, Is.Not.Null);
        Assert.That(ReferenceEquals(result, _gameObject.Get<TestComponent>()), Is.True);
    }

    [Test]
    public void Get_WithExistingComponent_ReturnsComponent()
    {
        _gameObject.Attach<TestComponent>();

        TestComponent component = _gameObject.Get<TestComponent>();

        Assert.That(component, Is.Not.Null);
        Assert.That(component.Value, Is.EqualTo("test"));
    }

    [Test]
    public void Get_WithNonExistingComponent_ThrowsComponentNotFound()
    {
        Assert.Throws<ComponentNotFound>(() => _gameObject.Get<TestComponent>());
    }

    [Test]
    public void Get_WithMultipleComponents_ReturnsCorrectComponent()
    {
        _gameObject.Attach<TestComponent>();
        _gameObject.Attach<TestComponent2>();

        TestComponent component1 = _gameObject.Get<TestComponent>();
        TestComponent2 component2 = _gameObject.Get<TestComponent2>();

        Assert.That(component1.Value, Is.EqualTo("test"));
        Assert.That(component2.Number, Is.EqualTo(42));
    }

    [Test]
    public void Get_AfterDetach_ThrowsComponentNotFound()
    {
        _gameObject.Attach<TestComponent>();
        _gameObject.Detach<TestComponent>();

        Assert.Throws<ComponentNotFound>(() => _gameObject.Get<TestComponent>());
    }

    [Test]
    public void Attach_WithInstance_AttachesProvidedInstance()
    {
        TestComponent instance = new TestComponent { Value = "custom" };

        _gameObject.Attach(instance);

        TestComponent retrieved = _gameObject.Get<TestComponent>();
        Assert.That(ReferenceEquals(retrieved, instance), Is.True);
        Assert.That(retrieved.Value, Is.EqualTo("custom"));
    }

    [Test]
    public void Attach_WithInstance_CallsOnAttach()
    {
        TestComponent instance = new TestComponent();

        _gameObject.Attach(instance);

        Assert.That(instance.OnAttachCalled, Is.True);
    }

    [Test]
    public void Attach_WithInstance_SetsOwner()
    {
        TestComponent instance = new TestComponent();

        _gameObject.Attach(instance);

        Assert.That(instance.HasOwner(), Is.True);
    }

    [Test]
    public void Attach_WithMultipleComponentsOfSameType_AttachesBoth()
    {
        _gameObject.Attach<TestComponent>();
        _gameObject.Attach<TestComponent>();

        var components = _gameObject.GetComponents<TestComponent>();
        Assert.That(components.Count, Is.EqualTo(2));
        Assert.That(components[0].OnAttachCalled, Is.True);
        Assert.That(components[1].OnAttachCalled, Is.True);
    }

    [Test]
    public void Attach_WithInstance_ReturnsComponent()
    {
        TestComponent instance = new TestComponent();

        TestComponent result = _gameObject.Attach(instance);

        Assert.That(ReferenceEquals(result, instance), Is.True);
    }

    [Test]
    public void Detach_Self_RemovesComponentFromOwner()
    {
        _gameObject.Attach<TestComponent>();
        TestComponent component = _gameObject.Get<TestComponent>();

        component.Detach();

        Assert.Throws<ComponentNotFound>(() => _gameObject.Get<TestComponent>());
    }

    [Test]
    public void Detach_Self_CallsOnDetach()
    {
        _gameObject.Attach<TestComponent>();
        TestComponent component = _gameObject.Get<TestComponent>();

        component.Detach();

        Assert.That(component.OnDetachCalled, Is.True);
    }

    [Test]
    public void DetachSibling_ByInstance_RemovesSpecificComponent()
    {
        TestComponent first = new TestComponent { Value = "first" };
        TestComponent second = new TestComponent { Value = "second" };
        _gameObject.Attach(first);
        _gameObject.Attach(second);

        first.DetachSibling(second);

        var components = _gameObject.GetComponents<TestComponent>();
        Assert.That(components.Count, Is.EqualTo(1));
        Assert.That(components[0].Value, Is.EqualTo("first"));
    }

    [Test]
    public void Detach_Self_WithoutOwner_DoesNothing()
    {
        TestComponent component = new TestComponent();

        Assert.DoesNotThrow(() => component.Detach());
    }

    [Test]
    public void AttachIfMissing_WhenNotPresent_AttachesComponent()
    {
        _gameObject.AttachIfMissing<TestComponent>();

        TestComponent component = _gameObject.Get<TestComponent>();
        Assert.That(component, Is.Not.Null);
        Assert.That(component.OnAttachCalled, Is.True);
    }

    [Test]
    public void AttachIfMissing_WhenAlreadyPresent_DoesNotAttachAgain()
    {
        _gameObject.Attach<TestComponent>();

        _gameObject.AttachIfMissing<TestComponent>();

        var components = _gameObject.GetComponents<TestComponent>();
        Assert.That(components.Count, Is.EqualTo(1));
    }

    [Test]
    public void AttachIfMissing_ReturnsComponent()
    {
        TestComponent result = _gameObject.AttachIfMissing<TestComponent>();

        Assert.That(ReferenceEquals(result, _gameObject.Get<TestComponent>()), Is.True);
    }

    [Test]
    public void AttachIfMissing_WhenAlreadyPresent_ReturnsExistingComponent()
    {
        _gameObject.Attach<TestComponent>();
        TestComponent original = _gameObject.Get<TestComponent>();

        TestComponent result = _gameObject.AttachIfMissing<TestComponent>();

        Assert.That(ReferenceEquals(result, original), Is.True);
        Assert.That(_gameObject.GetComponents<TestComponent>().Count, Is.EqualTo(1));
    }

    [Test]
    public void RemoveOwner_RemovesGameObjectFromWorld()
    {
        _gameObject.Attach<TestComponent>();
        TestComponent component = _gameObject.Get<TestComponent>();

        component.RemoveOwner();

        Assert.That(_gameObject.State, Is.EqualTo(GameObjectState.Removed));
    }

    [Test]
    public void RemoveOwner_DetachesAllComponents()
    {
        _gameObject.Attach<TestComponent>();
        _gameObject.Attach<TestComponent2>();
        TestComponent component = _gameObject.Get<TestComponent>();

        component.RemoveOwner();

        Assert.That(component.OnDetachCalled, Is.True);
    }

    [Test]
    public void State_NewGameObject_IsAlive()
    {
        Assert.That(_gameObject.State, Is.EqualTo(GameObjectState.Alive));
    }

    [Test]
    public void State_AfterRemove_IsRemoved()
    {
        _world.RemoveGameObject(_gameObject);

        Assert.That(_gameObject.State, Is.EqualTo(GameObjectState.Removed));
    }

    [Test]
    public void State_DuringOnDetach_IsRemoving()
    {
        GameObjectState? stateDuringDetach = null;
        StateCapturingComponent component = new StateCapturingComponent();
        component.CaptureAction = c => stateDuringDetach = c.Owner.State;
        _gameObject.Attach(component);

        _world.RemoveGameObject(_gameObject);

        Assert.That(stateDuringDetach, Is.EqualTo(GameObjectState.Removing));
    }

    [Test]
    public void State_DuringRemovedEvent_WorldIsAccessible()
    {
        GameWorld? worldDuringEvent = null;
        _gameObject.Removed += go => worldDuringEvent = go.World;

        _world.RemoveGameObject(_gameObject);

        Assert.That(worldDuringEvent, Is.SameAs(_world));
    }

    [Test]
    public void Attach_AfterRemove_Throws()
    {
        _world.RemoveGameObject(_gameObject);

        Assert.Throws<InvalidOperationException>(() => _gameObject.Attach<TestComponent>());
    }

    [Test]
    public void Attach_DuringRemoving_Throws()
    {
        StateCapturingComponent component = new StateCapturingComponent();
        component.CaptureAction = c => c.Owner.Attach<TestComponent2>();
        _gameObject.Attach(component);

        Assert.Throws<InvalidOperationException>(() => _world.RemoveGameObject(_gameObject));
    }

    [Test]
    public void Detach_AfterRemove_DoesNotThrow()
    {
        _gameObject.Attach<TestComponent>();

        _world.RemoveGameObject(_gameObject);

        Assert.DoesNotThrow(() => _gameObject.Detach<TestComponent>());
    }

    [Test]
    public void DetachAll_AfterRemove_DoesNotThrow()
    {
        _gameObject.Attach<TestComponent>();

        _world.RemoveGameObject(_gameObject);

        Assert.DoesNotThrow(() => _gameObject.DetachAll());
    }

    [Test]
    public void World_AfterRemove_Throws()
    {
        _world.RemoveGameObject(_gameObject);

        Assert.Throws<InvalidOperationException>(() => { GameWorld w = _gameObject.World; });
    }

    [Test]
    public void Attach_CallsOnReady()
    {
        _gameObject.Attach<TestComponent>();

        TestComponent component = _gameObject.Get<TestComponent>();
        Assert.That(component.OnReadyCalled, Is.True);
    }

    [Test]
    public void Attach_WithInstance_CallsOnReady()
    {
        TestComponent instance = new TestComponent();

        _gameObject.Attach(instance);

        Assert.That(instance.OnReadyCalled, Is.True);
    }
}
