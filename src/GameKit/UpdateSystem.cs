using GameKit.Collections;

namespace GameKit;

public class UpdateTag
{
    private UpdateTag() { }
}

public class UpdateSystem: IUpdatable
{
    private DenseSlotMapStruct<Handle64<UpdateTag>, Action> _updateActions = new();
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

    public Handle64<UpdateTag> Add(Action action)
    {
        return _updateActions.Add(action);
    }
    
    public void Remove(Handle64<UpdateTag> handle)
    {
        _updateActions.Remove(handle);
    }
}