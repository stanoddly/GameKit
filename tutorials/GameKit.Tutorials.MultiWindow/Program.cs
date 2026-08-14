using GameKit.App;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content")
            .UseDefaultRendering();

        builder.AddSingleton(new WindowConfig
        {
            Size = (640, 480),
            Title = "Main menu - click to start"
        });
        builder.OnStart(static (IStageManager stages) => stages.Load(MenuStage.Configure));

        using IGameKitApp gameKitApp = builder.Build();
        return gameKitApp.Run();
    }
}
