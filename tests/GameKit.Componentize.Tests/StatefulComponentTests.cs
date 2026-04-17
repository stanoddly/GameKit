using GameKit.DependencyInjection;

namespace GameKit.Componentize.Tests;

public class TestStatefulComponent : StatefulComponent<TestStatefulComponent, TestStatefulComponent.TestState>
{
    public TestStatefulComponent(TestState initialState) : base(initialState) { }

    public new TestState ChangeState(TestState newState) => base.ChangeState(newState);
    public new TestState CurrentState => base.CurrentState;

    public abstract class TestState : State { }

    public class IdleState : TestState
    {
        public bool Entered { get; private set; }
        public bool Exited { get; private set; }

        public override void Enter(TestStatefulComponent context) => Entered = true;
        public override void Exit(TestStatefulComponent context) => Exited = true;
    }

    public class ActiveState : TestState
    {
        public string Target { get; }
        public bool Entered { get; private set; }
        public bool Exited { get; private set; }

        public ActiveState(string target) => Target = target;

        public override void Enter(TestStatefulComponent context) => Entered = true;
        public override void Exit(TestStatefulComponent context) => Exited = true;
    }
}

public class StatefulComponentTests
{
    private GameWorld _world;
    private GameObject _gameObject;

    [SetUp]
    public void Setup()
    {
        _world = new GameWorld(ServiceProvider.Empty);
        _gameObject = _world.CreateGameObject();
    }

    [Test]
    public void Attach_CallsEnterOnInitialState()
    {
        var idle = new TestStatefulComponent.IdleState();
        var machine = new TestStatefulComponent(idle);

        _gameObject.Attach(machine);

        Assert.That(idle.Entered, Is.True);
    }

    [Test]
    public void Detach_CallsExitOnCurrentState()
    {
        var idle = new TestStatefulComponent.IdleState();
        var machine = new TestStatefulComponent(idle);
        _gameObject.Attach(machine);

        _gameObject.Detach<TestStatefulComponent>();

        Assert.That(idle.Exited, Is.True);
    }

    [Test]
    public void ChangeState_CallsExitOnOldAndEnterOnNew()
    {
        var idle = new TestStatefulComponent.IdleState();
        var machine = new TestStatefulComponent(idle);
        _gameObject.Attach(machine);

        var active = new TestStatefulComponent.ActiveState("target");
        machine.ChangeState(active);

        Assert.That(idle.Exited, Is.True);
        Assert.That(active.Entered, Is.True);
    }

    [Test]
    public void ChangeState_UpdatesCurrentState()
    {
        var idle = new TestStatefulComponent.IdleState();
        var machine = new TestStatefulComponent(idle);
        _gameObject.Attach(machine);

        var active = new TestStatefulComponent.ActiveState("target");
        machine.ChangeState(active);

        Assert.That(machine.CurrentState, Is.SameAs(active));
    }
}
