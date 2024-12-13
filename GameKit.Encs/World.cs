using System.Collections.Concurrent;
using GameKit.Collections;

namespace GameKit.Encs;

public interface ILifecycleFrameEnd
{
    void FrameEnd();
}

public readonly record struct EntitiesDeletedArg(Handle[] Handles);

public class World: ILifecycleFrameEnd
{
    private readonly EventBus _eventBus;
    private int _entityCounter;
    private readonly ConcurrentStack<Handle> _handlesToRemove = new();

    public World(EventBus eventBus)
    {
        this._eventBus = eventBus;
    }

    public Handle CreateEntity()
    {
        return (Handle)_entityCounter++;
    }

    public void RemoveEntityDeferred(Handle handle)
    {
        _handlesToRemove.Push(handle);
        
        _handlesToRemove.Clear();
    }


    public void FrameEnd()
    {
        if (_handlesToRemove.Count > 0)
        {
            Handle[] handles = _handlesToRemove.ToArray();
            
            _eventBus.PublishEvent(new EntitiesDeletedArg(handles));
            
            _handlesToRemove.Clear();
        }
    }
}