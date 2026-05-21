using GameKit.DependencyInjection;

namespace GameKit.App;

public class GameKitApp : IGameKitApp
{
    public ServiceProvider ServiceProvider { get; }
    private readonly UpdateRegistry _updateRegistry;

    internal GameKitApp(ServiceProvider serviceProvider, UpdateRegistry updateRegistry)
    {
        ServiceProvider = serviceProvider;
        _updateRegistry = updateRegistry;
    }

    public T GetRequiredService<T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public int Run()
    {
        GameKitFrameContext frameContext = ServiceProvider.GetRequiredService<GameKitFrameContext>();
        EventService eventService = ServiceProvider.GetRequiredService<EventService>();
        AppControl appControl = ServiceProvider.GetRequiredService<AppControl>();
        IRenderManager rootRenderer = ServiceProvider.GetRequiredService<IRenderManager>();

        while (true)
        {
            // in the very beginning of the frame adjust time and delta
            frameContext.StartFrame();
            // then process events
            eventService.Process();

            foreach (IUpdatable updatable in _updateRegistry.Snapshot())
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
