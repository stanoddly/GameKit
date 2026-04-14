namespace GameKit.DI;

public class ServiceRegistrar<T> where T : class
{
    private readonly ServiceCollection _collection;

    internal ServiceRegistrar(ServiceCollection collection)
    {
        _collection = collection;
    }

    public ServiceRegistrar<T> As<TTarget>() where TTarget : class
    {
        _collection.RegisterAlias<T, TTarget>();
        return this;
    }
}
