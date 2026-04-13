using GameKit.Common;
using GameKit.Encs;
using Yak;

namespace GameKit.Modules;

[Module]
public abstract partial class GameKitModule
{
    public List<IStartable> Startables { get; } = new();
    public List<IUpdatable> Updatables { get; } = new();
    public EventBus EventBus { get; } = new();

    [OnActivate]
    protected void TrackStartable(IStartable startable)
    {
        Startables.Add(startable);
    }

    [OnActivate]
    protected void TrackUpdatable(IUpdatable updatable)
    {
        Updatables.Add(updatable);
    }

    [OnActivate]
    protected void SubscribeEventBus(object obj)
    {
        EventBus.Subscribe(obj);
    }
}
