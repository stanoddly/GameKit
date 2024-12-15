namespace GameKit.Modules;

public interface IPreparable
{
    void Prepare();
}

public interface IUpdatable
{
    void Update();
}

public interface IDrawable
{
    void Draw();
}

public class GameModule: IDisposable
{
    private readonly List<IUpdatable> _updatables;
    private readonly List<IDrawable> _drawables;
    private readonly Dictionary<Type, object> _services;

    public IReadOnlyDictionary<Type, object> Services => _services;

    public GameModule(List<IUpdatable> updatables, List<IDrawable> drawables, Dictionary<Type, object> services)
    {
        _updatables = updatables;
        _drawables = drawables;
        _services = services;
        //_frameContext = (FrameContext)services[typeof(FrameContext)];
    }

    public void Draw()
    {
        foreach (IDrawable drawable in _drawables)
        {
            drawable.Draw();
        }
    }

    public void Update()
    {
        foreach (IUpdatable updatable in _updatables)
        {
            updatable.Update();
        }
    }

    public void BeginFrame(float delta)
    {
        //_frameContext.Update(delta);
    }

    public void Dispose()
    {
        foreach (object obj in _services.Values)
        {
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}