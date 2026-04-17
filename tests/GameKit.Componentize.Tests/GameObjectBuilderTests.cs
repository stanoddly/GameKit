using GameKit.DependencyInjection;

namespace GameKit.Componentize.Tests;

public class SiblingCapturingComponent : GameComponent
{
    public TestComponent? SiblingDuringOnAttach { get; private set; }
    public TestComponent? SiblingDuringOnReady { get; private set; }

    protected override void OnAttach()
    {
        SiblingDuringOnAttach = TryGetSibling<TestComponent>();
    }

    protected override void OnReady()
    {
        SiblingDuringOnReady = TryGetSibling<TestComponent>();
    }
}

public class LifecycleOrderComponent : GameComponent
{
    public List<string> Log { get; }

    public LifecycleOrderComponent(List<string> log)
    {
        Log = log;
    }

    public string Name { get; set; } = "";

    protected override void OnAttach()
    {
        Log.Add($"{Name}:OnAttach");
    }

    protected override void OnReady()
    {
        Log.Add($"{Name}:OnReady");
    }
}

public class GameObjectBuilderTests
{
    GameWorld _world;

    [SetUp]
    public void Setup()
    {
        _world = new GameWorld(ServiceProvider.Empty);
    }

    [Test]
    public void Build_CreatesGameObjectWithComponents()
    {
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject gameObject = builder
            .With<TestComponent>()
            .With<TestComponent2>()
            .Build();

        Assert.That(gameObject.TryGet<TestComponent>(), Is.Not.Null);
        Assert.That(gameObject.TryGet<TestComponent2>(), Is.Not.Null);
    }

    [Test]
    public void Build_CallsOnAttachOnAllComponents()
    {
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject gameObject = builder
            .With<TestComponent>()
            .Build();

        TestComponent component = gameObject.Get<TestComponent>();
        Assert.That(component.OnAttachCalled, Is.True);
    }

    [Test]
    public void Build_CallsOnReadyOnAllComponents()
    {
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject gameObject = builder
            .With<TestComponent>()
            .Build();

        TestComponent component = gameObject.Get<TestComponent>();
        Assert.That(component.OnReadyCalled, Is.True);
    }

    [Test]
    public void Build_CallsOnAttachBeforeOnReady()
    {
        List<string> log = new();
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        builder
            .With(new LifecycleOrderComponent(log) { Name = "A" })
            .With(new LifecycleOrderComponent(log) { Name = "B" })
            .Build();

        Assert.That(log, Is.EqualTo(new[] { "A:OnAttach", "B:OnAttach", "A:OnReady", "B:OnReady" }));
    }

    [Test]
    public void Build_SiblingsGuaranteedDuringOnReady()
    {
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject gameObject = builder
            .With(new SiblingCapturingComponent())
            .With<TestComponent>()
            .Build();

        SiblingCapturingComponent capturing = gameObject.Get<SiblingCapturingComponent>();
        Assert.That(capturing.SiblingDuringOnReady, Is.Not.Null);
    }

    [Test]
    public void Build_WithInstance_AttachesProvidedInstance()
    {
        TestComponent instance = new TestComponent { Value = "custom" };
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject gameObject = builder
            .With(instance)
            .Build();

        Assert.That(ReferenceEquals(gameObject.Get<TestComponent>(), instance), Is.True);
    }

    [Test]
    public void Build_ReturnsGameObjectRegisteredInWorld()
    {
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject gameObject = builder.With<TestComponent>().Build();

        Assert.That(gameObject.State, Is.EqualTo(GameObjectState.Alive));
    }

    [Test]
    public void Build_IsReusable()
    {
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject first = builder.With<TestComponent>().Build();
        GameObject second = builder.With<TestComponent2>().Build();

        Assert.That(first.TryGet<TestComponent>(), Is.Not.Null);
        Assert.That(first.TryGet<TestComponent2>(), Is.Null);
        Assert.That(second.TryGet<TestComponent2>(), Is.Not.Null);
        Assert.That(second.TryGet<TestComponent>(), Is.Null);
    }

    [Test]
    public void Build_WithNoComponents_CreatesEmptyGameObject()
    {
        GameObjectBuilder builder = _world.CreateGameObjectBuilder();

        GameObject gameObject = builder.Build();

        Assert.That(gameObject.Count(), Is.EqualTo(0));
    }

}
