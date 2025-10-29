namespace GameKit.Ioc;

public readonly struct GameModuleRegistrar<TService> where TService: class
{
    private readonly GameModuleBuilder _builder;

    internal GameModuleRegistrar(GameModuleBuilder builder)
    {
        _builder = builder;
    }
    
    public GameModuleRegistrar<TService> As<TTarget>() where TTarget : class
    {
        if (!typeof(TTarget).IsAssignableFrom(typeof(TService)))
        {
            throw new Exception();
        }
        
        _builder.RegisterAs<TService, TTarget>();
        return this;
    }
}
