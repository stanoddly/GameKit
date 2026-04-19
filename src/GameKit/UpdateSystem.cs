using GameKit.Collections;

namespace GameKit;

public class UpdateTag
{
    private UpdateTag() { }
}

public class UpdateSystem : IUpdatable, ITickRegistrar
{
    private DenseSlotMapStruct<Handle<UpdateTag>, Action> _updateActions = new();
    private List<Action> _temp = new();

    public void Update()
    {
        ReadOnlySpan<Action> actions = _updateActions.Values1;

        _temp.Clear();
        _temp.AddRange(actions);

        foreach (Action action in _temp)
        {
            action();
        }
    }

    public Handle<UpdateTag> Add(Action action)
    {
        return _updateActions.Add(action);
    }

    public void Remove(Handle<UpdateTag> handle)
    {
        _updateActions.Remove(handle);
    }

    Action ITickRegistrar.Register(Action tick)
    {
        Handle<UpdateTag> handle = Add(tick);
        return () => Remove(handle);
    }
}