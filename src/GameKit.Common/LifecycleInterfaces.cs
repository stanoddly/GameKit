namespace GameKit;

public interface IStartable
{
    void Start();
}

public interface IUpdatable
{
    void Update();
}

public interface ITickRegistrar
{
    // Returns an action that, when invoked, removes the registration.
    Action Register(Action tick);
}
