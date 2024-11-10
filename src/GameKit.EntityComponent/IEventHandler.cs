namespace GameKit.EntityComponent;

public interface IEventHandler<TEventArgs> where TEventArgs: struct
{
    void Handle(GameObject gameObject, in TEventArgs args);
}
