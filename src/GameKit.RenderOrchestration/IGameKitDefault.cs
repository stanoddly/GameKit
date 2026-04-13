using GameKit.App;
using GameKit.Common;
using GameKit.Modules;
using Yak;

namespace GameKit.RenderOrchestration;

[Module]
public interface IGameKitDefault : IGameKitCore, IDefaultRenderOrchestration<DefaultRenderContext>
{
    [Singleton<DefaultRenderContextProvider>]
    new IRenderContextProvider<DefaultRenderContext> RenderContextProvider { get; }

    int Run()
    {
        ResolveAll();

        GameKitFrameContext frameContext = FrameContext;
        EventService eventService = EventService;
        AppControl appControl = AppControl;
        IRenderManager renderManager = RenderManager;

        for (int i = Startables.Count - 1; i >= 0; i--)
        {
            Startables[i].Start();
        }

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
