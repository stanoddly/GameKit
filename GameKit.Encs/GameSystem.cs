using GameKit.Collections;

namespace GameKit.Encs;

public abstract class GameSystem<TComponent>
{
    public abstract void CreateComponent(Handle handle, in TComponent component);
    public abstract bool RemoveComponent(Handle handle);
    public abstract bool TryGetComponent(Handle handle, out TComponent component);

    public TComponent GetComponent(Handle handle)
    {
        if (!TryGetComponent(handle, out TComponent component))
        {
            throw new HandleNullException(handle.ToString());
        }

        return component;
    }
}
