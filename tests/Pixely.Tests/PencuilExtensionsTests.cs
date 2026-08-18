using Pixely.App;
using Pixely.Gpu;
using Pixely.Pencuil;
using Pixely.RenderOrchestration;

namespace Pixely.Tests;

public sealed class PencuilExtensionsTests
{
    [Test]
    public void UsePencuil_CustomRenderContext_RegistersRenderer()
    {
        PixelyAppBuilder builder = new();

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
