using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;

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
        IRenderManager rootRenderer = ServiceProvider.GetRequiredService<IRenderManager>();
        ServiceRegistry<IUpdatable> updatables = ServiceProvider.GetRequiredService<ServiceRegistry<IUpdatable>>();
        StageManager stageManager = ServiceProvider.GetRequiredService<StageManager>();

        updatables.Sort(static (left, right) =>
        {
            int leftOrder = left is IOrderable leftOrderable ? leftOrderable.Order : 0;
            int rightOrder = right is IOrderable rightOrderable ? rightOrderable.Order : 0;
            return leftOrder.CompareTo(rightOrder);
        });

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
            rootRenderer.Execute();
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
