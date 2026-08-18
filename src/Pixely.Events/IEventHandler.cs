namespace Pixely.Events;

public interface IEventHandler<TEventArgs>
{
    void Process(TEventArgs args);
}
