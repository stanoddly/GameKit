using GameKit.App;
using GameKit.Common;
using GameKit.Modules;
using Yak;

namespace GameKit.RenderOrchestration;

[Module]
public interface IGameKitDefault : IDefaultRenderOrchestration<DefaultRenderContext>
{
    [Singleton<DefaultRenderContextProvider>]
    new IRenderContextProvider<DefaultRenderContext> RenderContextProvider { get; }

    // These are declared on GameKitModule (base class) — referenced here for the game loop
    AppControl AppControl { get; }
    GameKitFrameContext FrameContext { get; }
    EventService EventService { get; }
    List<IUpdatable> Updatables { get; }

    void ResolveAll();

    int Run()
    {
        ResolveAll();

        GameKitFrameContext frameContext = FrameContext;
        EventService eventService = EventService;
        AppControl appControl = AppControl;
        IRenderManager renderManager = RenderManager;

        while (true)
        {
            frameContext.StartFrame();
            eventService.Process();

            foreach (IUpdatable updatable in Updatables)
            {
                updatable.Update();
            }

            if (appControl.QuitRequested)
            {
                return 0;
            }

            renderManager.Execute();
        }
    }
}
