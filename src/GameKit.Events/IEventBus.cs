using System.Diagnostics.CodeAnalysis;

namespace GameKit.Events;

public interface IEventBus
{
    void Subscribe<TSubscriber>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TSubscriber instance)
        where TSubscriber : notnull;

    void Subscribe(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type);

    void Subscribe<TEventArgs>(IEventHandler<TEventArgs> obj);

    void Unsubscribe<TEventArgs>(IEventHandler<TEventArgs> obj);

    void Unsubscribe<TSubscriber>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TSubscriber instance)
        where TSubscriber : notnull;

    void Unsubscribe(
        object instance,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type);

    void PublishEvent<TEventArgs>(TEventArgs args);
}
