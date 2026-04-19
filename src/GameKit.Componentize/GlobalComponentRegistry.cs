using System.Runtime.CompilerServices;

namespace GameKit.Componentize;

public class GlobalComponentRegistry
{
    private Dictionary<Type, GameComponent>? _components;

    public void Add<T>(T component) where T : GameComponent
    {
        _components ??= new();

        if (!_components.TryAdd(typeof(T), component))
        {
            throw new InvalidOperationException($"A component of type {typeof(T).Name} is already registered.");
        }
    }

    public void Remove<T>(T component) where T : GameComponent
    {
        if (_components == null || !_components.TryGetValue(typeof(T), out GameComponent? existing) || existing != component)
        {
            throw new InvalidOperationException($"Component is not registered as {typeof(T).Name}.");
        }

        _components.Remove(typeof(T));
    }

    public T Get<T>() where T : GameComponent
    {
        if (_components != null && _components.TryGetValue(typeof(T), out GameComponent? component))
        {
            return Unsafe.As<GameComponent, T>(ref component);
        }

        throw new InvalidOperationException($"No component registered as {typeof(T).Name}.");
    }

    public T? TryGet<T>() where T : GameComponent
    {
        if (_components != null && _components.TryGetValue(typeof(T), out GameComponent? component))
        {
            return Unsafe.As<GameComponent, T>(ref component);
        }

        return null;
    }
}
