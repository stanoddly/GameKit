using System.Runtime.CompilerServices;

namespace GameKit.Encs;

public abstract class System<TKey, TComponent>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TComponent InternalCreate(TKey key, TComponent component)
    {
        return Create(key, component);
    }

    protected abstract TComponent Create(TKey key, TComponent component);
    
    public abstract TComponent Get(TKey key);
    public abstract bool Remove(TKey key);
}


public class World<TKey>
{
    // TODO: sparse map<TKey, BitSet>
    public void AddComponent<TComponent>(TKey key, TComponent component)
    {
        
    }

    public void AddSystem<TComponent>(System<TKey, TComponent> system)
    {
        
    }
}

public readonly record struct Entity<TKey>(TKey Key, World<TKey> World)
{
    public void AddComponent<TComponent>(TComponent component)
    {
        World.AddComponent(Key, component);
    }
}