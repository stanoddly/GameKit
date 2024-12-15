using GameKit.Collections;

namespace GameKit.Encs;

public abstract class GameSystem<TComponent>
{
    public abstract void SetComponent(Handle handle, in TComponent component);
    public abstract void RemoveComponent(Handle handle);
    public abstract bool TryGetComponent(Handle handle, out TComponent component);

    public virtual bool Exists(Handle handle)
    {
        if (TryGetComponent(handle, out TComponent component))
        {
            return true;
        }

        return false;        
    }

    public virtual TComponent GetComponent(Handle handle)
    {
        if (!TryGetComponent(handle, out TComponent component))
        {
            throw new HandleNullException(handle.ToString());
        }

        return component;
    }

    public virtual bool UpdateComponent(Handle handle, in TComponent component)
    {
        if (!Exists(handle))
        {
            return false;
        }
        
        SetComponent(handle, in component);
        return true;
    }

    public virtual bool CreateComponent(Handle handle, in TComponent component)
    {
        if (Exists(handle))
        {
            return false;
        }

        SetComponent(handle, in component);
        return true;
    }
}
