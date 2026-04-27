namespace GameKit.Events;

public interface IEventHandler<TEventArgs>
{
    void Process(TEventArgs args);
}
