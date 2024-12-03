using System.Collections;
using System.Runtime.CompilerServices;
using GameKit.Collections;

namespace GameKit.Encs;

public abstract class System<TKey, TComponent> where TComponent : struct
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TComponent InternalAddComponent(TKey key, in TComponent component)
    {
        return AddComponent(key, component);
    }

    protected abstract TComponent AddComponent(TKey key, in TComponent component);
    
    public abstract TComponent GetComponent(TKey key);
    public abstract bool RemoveComponent(TKey key);
}

public interface IKeyManager<TKey> where TKey : IKey<TKey>
{
    
    TKey RotateKey(TKey key);
    TKey ReleaseKey(TKey key);
    TKey ObtainKey();
}

public class World<TKey> where TKey : IKey<TKey>
{
    private FastListStruct<object> _systems = new();
    private MultiMapStruct<TKey, BitSetStruct> _componentsPerEntity = new();
    private FastListStruct<TKey> _toRecycleKeys = new();
    private readonly IKeyManager<TKey> _keyManager;

    public World(IKeyManager<TKey> keyManager)
    {
        _keyManager = keyManager;
    }
    
    public void PrivateAddComponent<TComponent>(TKey key, ref BitSetStruct components, in TComponent component) where TComponent : struct
    {
        components
            
        nuint componentId = ComponentTypeId<TComponent>.Id;

        object rawSystem = _systems[componentId];
        System<TKey, TComponent> system = Unsafe.As<System<TKey, TComponent>>(rawSystem);

        system.InternalAddComponent(key, component);
    }

    public void AddComponent<TComponent>(TKey key, in TComponent component) where TComponent : struct
    {
        nuint componentId = ComponentTypeId<TComponent>.Id;

        object rawSystem = _systems[componentId];
        System<TKey, TComponent> system = Unsafe.As<System<TKey, TComponent>>(rawSystem);

        system.InternalAddComponent(key, component);
    }

    public Entity<TKey> CreateEntity<TComponent1, TComponent2>(in TComponent1 component1, in TComponent2 component2)
        where TComponent1 : struct
        where TComponent2 : struct
    {
        TKey key = _keyManager.ObtainKey();
        
        AddComponent()
    }
    
    public bool AddComponent<TComponent1, TComponent2>(TKey key, in TComponent1 component1, in TComponent2 component2)
        where TComponent1 : struct
        where TComponent2 : struct
    {
        if (!_componentsPerEntity.TryGet(key, out BitSetStruct components))
        {
            return false;
        }
        
        nuint componentId1 = ComponentTypeId<TComponent1>.Id;
        nuint componentId2 = ComponentTypeId<TComponent1>.Id;
        
        object rawSystem1 = _systems[componentId1];
        System<TKey, TComponent1> system1 = Unsafe.As<System<TKey, TComponent1>>(rawSystem1);
        system1.InternalAddComponent(key, component1);
        components.Set(componentId1, true);
        
        object rawSystem2 = _systems[componentId2];
        System<TKey, TComponent2> system2 = Unsafe.As<System<TKey, TComponent2>>(rawSystem2);
        system2.InternalAddComponent(key, component2);
        components.Set(componentId1, true);

        return false;
    }

    public void AddSystem<TComponent>(System<TKey, TComponent> system)
        where TComponent : struct
    {
        int componentId = ComponentTypeId<TComponent>.Id;

        int targetCount = componentId + 1;
        
        _systems.ResizeFill(targetCount, null!);
        
        _systems[componentId] = system;
    }
    
    
}

public readonly record struct Entity<TKey>(TKey Key, World<TKey> World)
{
    public void AddComponent<TComponent>(in TComponent component) where TComponent : struct
    {
        World.AddComponent(Key, component);
    }
}