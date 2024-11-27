using System.Collections;
using System.Runtime.CompilerServices;
using GameKit.Collections;
using GameKit.Utilities;

namespace GameKit.ComponentSystem;

public abstract class GameSystem<TEntityKey> where TEntityKey: struct, IKey<TEntityKey>
{
    public abstract void Remove(TEntityKey id);
}

public interface IGameSystem<TEntityKey, TComponent> where TEntityKey: struct, IKey<TEntityKey>
{
    void SetComponent(TEntityKey id, in TComponent component);

    void SetComponents(ReadOnlySpan<TEntityKey> keys, ReadOnlySpan<TComponent> component)
    {
        if (keys.Length != component.Length)
        {
            throw new ArgumentException("Keys and Components must have the same length.");
        }

        for (int i = 0; i < keys.Length; i++)
        {
            SetComponent(keys[i], component[i]);
        }
    }

    void RemoveComponent(TEntityKey key);

    void RemoveComponents(ReadOnlySpan<TEntityKey> keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            RemoveComponent(keys[i]);
        }
    }
}
