using GameKit.App;
using GameKit.Gpu;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public sealed class PencuilExtensionsTests
{
    [Test]
    public void UsePencuil_CustomRenderContext_RegistersRenderer()
    {
        GameKitAppBuilder builder = new();

        builder.UsePencuil<CustomRenderContext>(new ViewScope(1));

        Assert.That(builder.IsRegistered<IRenderer<CustomRenderContext>>(), Is.True);
    }

    private sealed class CustomRenderContext : IRenderContext
    {
        public CommandBuffer CommandBuffer => null!;
        public Texture ColorTarget => null!;

        public void Dispose()
        {
        }
    }
}
