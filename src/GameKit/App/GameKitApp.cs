using GameKit.DependencyInjection;

namespace GameKit.App;

public class GameKitApp : IGameKitApp
{
    public ServiceProvider ServiceProvider { get; }
    private readonly List<IStartable> _startables;
    private readonly List<IUpdatable> _updatables;

    public GameKitApp(ServiceProvider serviceProvider, List<IStartable> startables, List<IUpdatable> updatables)
    {
        ServiceProvider = serviceProvider;
        _startables = startables;
        _updatables = updatables;
    }

    public T GetService<T>() where T : class
    {
        return ServiceProvider.GetService<T>();
    }

    public int Run()
    {
        GameKitFrameContext frameContext = ServiceProvider.GetService<GameKitFrameContext>();
        EventService eventService = ServiceProvider.GetService<EventService>();
        AppControl appControl = ServiceProvider.GetService<AppControl>();
        IRenderManager rootRenderer = ServiceProvider.GetService<IRenderManager>();

        for (int i = _startables.Count - 1; i >= 0; i--)
        {
            _startables[i].Start();
        }

        while (true)
        {
            // in the very beginning of the frame adjust time and delta
            frameContext.StartFrame();
            // then process events
            eventService.Process();

            foreach (IUpdatable updatable in _updatables)
            {
                updatable.Update();
            }

            if (appControl.QuitRequested)
            {
                return 0;
            }

            // finally render
            rootRenderer.Execute();
        }
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}
