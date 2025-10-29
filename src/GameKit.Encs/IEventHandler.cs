namespace GameKit.Encs;

public interface IEventHandler<TEventArgs>
{
    void Process(TEventArgs args);
}
