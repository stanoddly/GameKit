namespace GameKit.Encs;

public interface IEventHandler<TEventArgs> where TEventArgs: struct
{
    void Handle(in TEventArgs args);
}
