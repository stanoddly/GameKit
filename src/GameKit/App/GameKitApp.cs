using System.Diagnostics.CodeAnalysis;
using GameKit.DependencyInjection;

namespace GameKit.App;

public class GameKitApp : IGameKitApp
{
    private bool _disposed;

    public ServiceProvider ServiceProvider { get; }

    internal GameKitApp(ServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public ServiceCollection CreateServiceCollection()
    {
        return ServiceProvider.CreateServiceCollection();
    }

    public T GetRequiredService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>() where T : class
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public int Run()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        GameKitFrameContext frameContext = ServiceProvider.GetRequiredService<GameKitFrameContext>();
        EventService eventService = ServiceProvider.GetRequiredService<EventService>();
        AppControl appControl = ServiceProvider.GetRequiredService<AppControl>();
        ServiceRegistry<RenderCoordinator> renderCoordinators =
            ServiceProvider.GetRequiredService<ServiceRegistry<RenderCoordinator>>();
        ServiceRegistry<IUpdatable> updatables = ServiceProvider.GetRequiredService<ServiceRegistry<IUpdatable>>();
        StageManager stageManager = ServiceProvider.GetRequiredService<StageManager>();
        WindowManager windowManager = ServiceProvider.GetRequiredService<WindowManager>();

        while (true)
        {
            // start the frame before applying queued stage transitions
            frameContext.StartFrame();
            stageManager.ApplyPendingTransition();
            windowManager.ApplyPendingDisposals();
            // then process events
            eventService.Process();

            foreach (IUpdatable updatable in updatables)
            {
                updatable.Update();
            }

            if (appControl.QuitRequested)
            {
                return 0;
            }

            // finally render
            foreach (RenderCoordinator renderCoordinator in renderCoordinators)
            {
                renderCoordinator.Execute();
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ServiceProvider.Dispose();
    }
}
