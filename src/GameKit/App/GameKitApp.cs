using GameKit.Common;

namespace GameKit.App;

public class GameKitApp : IGameKitApp
{
    public IServiceProvider ServiceProvider { get; }
    private readonly List<IStartable> _startables;
    private readonly List<IUpdatable> _updatables;
    private readonly List<IDisposable> _disposables;

    public GameKitApp(IServiceProvider serviceProvider, List<IStartable> startables, List<IUpdatable> updatables, List<IDisposable> disposables)
    {
        ServiceProvider = serviceProvider;
        _startables = startables;
        _updatables = updatables;
        _disposables = disposables;
    }

    public TService GetMandatoryService<TService>()
    {
        return ServiceProvider.GetMandatoryService<TService>();
    }

    public int Run()
    {
        GameKitFrameContext frameContext = ServiceProvider.GetMandatoryService<GameKitFrameContext>();
        EventService eventService = ServiceProvider.GetMandatoryService<EventService>();
        AppControl appControl = ServiceProvider.GetMandatoryService<AppControl>();
        IRenderManager rootRenderer = ServiceProvider.GetMandatoryService<IRenderManager>();

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
        for (int i = _disposables.Count - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }
    }
}