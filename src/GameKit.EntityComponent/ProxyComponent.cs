namespace GameKit.EntityComponent;

public readonly record struct MyKey;



public class World
{
    private Dictionary<Type, object> _systems = new();

    public void Add(MyKey myKey, object component)
    {
        
    }
}

public readonly record struct Entity(long EntityKey, World World);

public readonly record struct ProxyComponent(object Obj, int Handle);
