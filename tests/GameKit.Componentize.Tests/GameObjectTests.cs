namespace GameKit.Componentize.Tests;

public class TestComponent : GameComponent
{
    public string Value { get; set; } = "test";
    public bool OnAttachCalled { get; private set; }
    public bool OnDetachCalled { get; private set; }

    protected override void OnAttach()
    {
        OnAttachCalled = true;
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

public class GameObjectTests
{
    GameWorld _world;
    GameObject _gameObject;

    [SetUp]
    public void Setup()
    {
        _world = new GameWorld();
        _gameObject = _world.CreateGameObject("test");
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
    public void Attach_ReturnsGameObject_ForChaining()
    {
        GameObject result = _gameObject.Attach<TestComponent>();
        
        Assert.That(ReferenceEquals(result, _gameObject), Is.True);
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
    public void Attach_WithInstance_ReturnsGameObject_ForChaining()
    {
        TestComponent instance = new TestComponent();
        
        GameObject result = _gameObject.Attach(instance);
        
        Assert.That(ReferenceEquals(result, _gameObject), Is.True);
    }
}
