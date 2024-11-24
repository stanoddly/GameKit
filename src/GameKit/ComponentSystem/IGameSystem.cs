using System.Collections;
using System.Runtime.CompilerServices;
using GameKit.Utilities;

namespace GameKit.ComponentSystem;

/*public class GameWorld
{
    private ulong _nextId = 1;
    private readonly Dictionary<ulong, BitArray> _entities = new();
    private readonly List<BaseGameSystem> _systems;

    internal GameWorld(List<BaseGameSystem> systems)
    {
        _systems = systems;
    }

    public void AddComponent<TComponent>(Entity id, in TComponent component)
    {
        Unsafe.As<GameSystem<TComponent>>(_systems[TypeId<TComponent>.Id]).Add(id, component);
    }

    private (Entity, BitArray) CreatePrivate()
    {
        ulong id = _nextId++;
        BitArray bitArray = new BitArray(_systems.Count);
        _entities.Add(id, bitArray);
        
        return (new Entity(id), bitArray);
    }

    public Entity CreateEntity()
    {
        return new Entity(0);
    }

    public Entity CreateEntity<TComponent1>(in TComponent1 component1)
    {
        (Entity id, BitArray bitArray) = CreatePrivate();
        AddComponent(id, in component1);
        
        bitArray.Set(TypeId<TComponent1>.Id, true);

        return id;
    }
    
    public Entity CreateEntity<TComponent1, TComponent2>(in TComponent1 component1, in TComponent2 component2)
    {
        (Entity id, BitArray bitArray) = CreatePrivate();
        AddComponent(id, in component1);
        AddComponent(id, in component2);
        
        bitArray.Set(TypeId<TComponent1>.Id, true);
        bitArray.Set(TypeId<TComponent2>.Id, true);
        
        return id;
    }

    public void DeleteEntity(Entity id)
    {
        if (_entities.Remove(id, out var bitArray))
        {
            for (int i = 0; i < bitArray.Length; i++)
            {
                bool value = bitArray[i];

                if (value)
                {
                    _systems[i].Remove(id);
                }
            }
        }
    }
}

public class GameWorldBuilder
{
    List<(int, BaseGameSystem)> _systems = new();

    public GameWorldBuilder AddSystem<TGameComponent>(GameSystem<TGameComponent> system)
    {
        _systems.Add((system.TypeId, system));
        return this;
    }

    public GameWorld Build()
    {
        List<BaseGameSystem> systems = new();

        foreach ((int typeId, BaseGameSystem system) in _systems)
        {
            systems[typeId] = system;
        }
        
        return new GameWorld(systems);
    }
}*/

public abstract class GameSystem<TEntityKey> where TEntityKey: struct, IKey<TEntityKey>
{
    public abstract void Remove(TEntityKey id);
}

public interface IGameSystem<TEntityKey, TComponent> where TEntityKey: struct, IKey<TEntityKey>
{
    void SetComponent(TEntityKey id, in TComponent component);
    void RemoveComponent(TEntityKey id);
}