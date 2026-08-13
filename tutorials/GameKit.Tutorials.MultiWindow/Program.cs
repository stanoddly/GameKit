using GameKit.App;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

static class Program
{
    static int Main(string[] args)
    {
        GameKitAppBuilder builder = new GameKitAppBuilder()
            .AddContentFromProjectDirectory("Content");

        builder.AddSingleton<GpuVertexBuffer<PositionVertex>>(static (GpuMemorySystem gpuMemorySystem) =>
            gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad));

        using IGameKitApp gameKitApp = builder.Build();

        ServiceCollection primaryWindowServices = gameKitApp.CreateServiceCollection();
        primaryWindowServices.AddWindow(new WindowOptions(
            Size: new Size<uint>(640, 480),
            Title: "Primary Window"));
        primaryWindowServices.UseDefaultRenderCoordinator();
        primaryWindowServices.AddSingleton<IRenderPhase<DefaultRenderContext>>(PrimaryRenderer.Create);
        primaryWindowServices.BuildServiceProvider();

        ServiceCollection secondaryWindowServices = gameKitApp.CreateServiceCollection();
        secondaryWindowServices.AddWindow(new WindowOptions(
            Size: new Size<uint>(480, 360),
            Title: "Secondary Window",
            StopGameOnClose: false));
        secondaryWindowServices.UseDefaultRenderCoordinator();
        secondaryWindowServices.AddSingleton<IRenderPhase<DefaultRenderContext>>(SecondaryWindowRenderer.Create);
        secondaryWindowServices.BuildServiceProvider();

        return gameKitApp.Run();
    }
}
