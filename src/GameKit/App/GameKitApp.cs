using GameKit.DependencyInjection;

namespace GameKit.App;

public class GameKitApp : IGameKitApp
{
    public ServiceProvider ServiceProvider { get; }

    internal GameKitApp(ServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
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
        UpdateLoop updateLoop = ServiceProvider.GetRequiredService<UpdateLoop>();

        while (true)
        {
            // in the very beginning of the frame adjust time and delta
            frameContext.StartFrame();
            // then process events
            eventService.Process();

            updateLoop.Update();

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
