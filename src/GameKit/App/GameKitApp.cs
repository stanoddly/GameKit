using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;
using GameKit.RenderOrchestration;

namespace GameKit.App;

public class GameKitApp : IGameKitApp
{
    public ServiceProvider ServiceProvider { get; }

    internal GameKitApp(ServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public T GetRequiredService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public int Run()
    {
        GameKitFrameContext frameContext = ServiceProvider.GetRequiredService<GameKitFrameContext>();
        EventService eventService = ServiceProvider.GetRequiredService<EventService>();
        AppControl appControl = ServiceProvider.GetRequiredService<AppControl>();
        RenderManager renderManager = ServiceProvider.GetRequiredService<RenderManager>();
        ServiceRegistry<IUpdatable> updatables = ServiceProvider.GetRequiredService<ServiceRegistry<IUpdatable>>();
        StageManager stageManager = ServiceProvider.GetRequiredService<StageManager>();

        while (true)
        {
            // start the frame before applying queued stage transitions
            frameContext.StartFrame();
            stageManager.ApplyPendingTransition();
            // then process events
            eventService.Process();

            Update(updatables);

            if (appControl.QuitRequested)
            {
                return 0;
            }

            // finally render
            renderManager.Execute();
        }
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }

    private static void Update(ServiceRegistry<IUpdatable> updatables)
    {
        foreach (IUpdatable updatable in updatables)
        {
            updatable.Update();
        }
    }
}
