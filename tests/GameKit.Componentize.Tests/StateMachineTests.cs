namespace GameKit.Componentize.Tests;

public class TestMachine : StateMachine<TestMachine, TestMachine.TestState>
{
    public TestMachine(TestState initialState) : base(initialState) { }

    public new TestState ChangeState(TestState newState) => base.ChangeState(newState);
    public new TestState CurrentState => base.CurrentState;

    public abstract class TestState : State { }

    public class IdleState : TestState
    {
        public bool Entered { get; private set; }
        public bool Exited { get; private set; }

        public override void Enter(TestMachine context) => Entered = true;
        public override void Exit(TestMachine context) => Exited = true;
    }

    public class ActiveState : TestState
    {
        public string Target { get; }
        public bool Entered { get; private set; }
        public bool Exited { get; private set; }

        public ActiveState(string target) => Target = target;

        public override void Enter(TestMachine context) => Entered = true;
        public override void Exit(TestMachine context) => Exited = true;
    }
}

public class StateMachineTests
{
    private GameObject _gameObject;

    [SetUp]
    public void Setup()
    {
        _gameObject = new GameObject();
    }

    [Test]
    public void Attach_CallsEnterOnInitialState()
    {
        var idle = new TestMachine.IdleState();
        var machine = new TestMachine(idle);

        _gameObject.Attach(machine);

        Assert.That(idle.Entered, Is.True);
    }

    [Test]
    public void Detach_CallsExitOnCurrentState()
    {
        var idle = new TestMachine.IdleState();
        var machine = new TestMachine(idle);
        _gameObject.Attach(machine);

        _gameObject.Detach<TestMachine>();

        Assert.That(idle.Exited, Is.True);
    }

    [Test]
    public void ChangeState_CallsExitOnOldAndEnterOnNew()
    {
        var idle = new TestMachine.IdleState();
        var machine = new TestMachine(idle);
        _gameObject.Attach(machine);

        var active = new TestMachine.ActiveState("target");
        machine.ChangeState(active);

        Assert.That(idle.Exited, Is.True);
        Assert.That(active.Entered, Is.True);
    }

    [Test]
    public void ChangeState_UpdatesCurrentState()
    {
        var idle = new TestMachine.IdleState();
        var machine = new TestMachine(idle);
        _gameObject.Attach(machine);

        var active = new TestMachine.ActiveState("target");
        machine.ChangeState(active);

        Assert.That(machine.CurrentState, Is.SameAs(active));
    }
}
