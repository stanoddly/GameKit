using System.Collections;
using GameKit.Collections;

namespace GameKit.ComponentSystem;

// TODO: empty constructor
public readonly record struct Entity<TEntityKey>(World<TEntityKey> World, TEntityKey Key) where TEntityKey : IKey<TEntityKey>;

public abstract class World<TEntityKey> where TEntityKey : IKey<TEntityKey>
{
    private Dictionary<Type, object> _systems = new();
    private Dictionary<TEntityKey, BitArray> _entities = new();

    public World(Dictionary<Type, object> systems)
    {
        _systems = systems;
    }

    public Entity<TEntityKey> CreateEntity()
    {
        TEntityKey key = CreateEntityKey();
        Entity<TEntityKey> entity = new Entity<TEntityKey>(this, key);

        return entity;
    }

    public abstract TEntityKey CreateEntityKey();

    public void AddComponent<TComponent>(Type componentType, object component)
    {
        
    }
}

public class WorldBuilder<TEntityKey> where TEntityKey : struct, IKey<TEntityKey>
{
    // TODO: OrderedDictionary
    private readonly Dictionary<Type, object> _systems = new();

    public WorldBuilder<TEntityKey> WithSystem<TComponent>(IGameSystem<TEntityKey, TComponent> system) 
    {
        _systems.Add(typeof(TComponent), system);
        return this;
    }

    public World<TEntityKey> Build()
    {
        
    }
}