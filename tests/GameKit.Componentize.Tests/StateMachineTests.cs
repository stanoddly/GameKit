namespace GameKit.Componentize.Tests;

public readonly record struct ActivateCommand(string Target);
public readonly record struct DeactivateCommand;

public class TestMachine : StateMachine<TestMachine, TestMachine.TestState>
{
    public TestMachine(TestState initialState) : base(initialState) { }

    public new TestState ChangeState(TestState newState) => base.ChangeState(newState);
    public new TestState CurrentState => base.CurrentState;

    public abstract class TestState : State { }

    public class IdleState : TestState, ICommandHandler<ActivateCommand>
    {
        public bool Entered { get; private set; }
        public bool Exited { get; private set; }

        public override void Enter(TestMachine context) => Entered = true;
        public override void Exit(TestMachine context) => Exited = true;

        public void Handle(TestMachine context, in ActivateCommand command)
        {
            context.ChangeState(new ActiveState(command.Target));
        }
    }

    public class ActiveState : TestState, ICommandHandler<DeactivateCommand>
    {
        public string Target { get; }
        public bool Entered { get; private set; }
        public bool Exited { get; private set; }

        public ActiveState(string target) => Target = target;

        public override void Enter(TestMachine context) => Entered = true;
        public override void Exit(TestMachine context) => Exited = true;

        public void Handle(TestMachine context, in DeactivateCommand command)
        {
            context.ChangeState(new IdleState());
        }
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

    [Test]
    public void Handle_DispatchesToCurrentState()
    {
        var machine = new TestMachine(new TestMachine.IdleState());
        _gameObject.Attach(machine);

        machine.Handle(new ActivateCommand("weapon"));

        Assert.That(machine.CurrentState, Is.TypeOf<TestMachine.ActiveState>());
        Assert.That(((TestMachine.ActiveState)machine.CurrentState).Target, Is.EqualTo("weapon"));
    }

    [Test]
    public void Handle_IgnoredWhenStateDoesNotHandleCommand()
    {
        var idle = new TestMachine.IdleState();
        var machine = new TestMachine(idle);
        _gameObject.Attach(machine);

        machine.Handle(new DeactivateCommand());

        Assert.That(machine.CurrentState, Is.SameAs(idle));
    }

    [Test]
    public void Handle_CommandCanTransitionAndNewStateHandlesDifferentCommands()
    {
        var machine = new TestMachine(new TestMachine.IdleState());
        _gameObject.Attach(machine);

        machine.Handle(new ActivateCommand("shield"));
        machine.Handle(new DeactivateCommand());

        Assert.That(machine.CurrentState, Is.TypeOf<TestMachine.IdleState>());
    }
}
