using GameKit.App;
using GameKit.Gpu;
using GameKit.Pencuil;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public class PencuilRegistrationTests
{
    [Test]
    public void UsePencuil_RegistersStateAndSystemPerRenderContext()
    {
        GameKitAppBuilder builder = new();

        builder.UsePencuil<DefaultRenderContext>();
        builder.UsePencuil<TestRenderContext>();

        Assert.Multiple(() =>
        {
            Assert.That(builder.IsRegistered<PencuilState<DefaultRenderContext>>(), Is.True);
            Assert.That(builder.IsRegistered<PencilSystem<DefaultRenderContext>>(), Is.True);
            Assert.That(builder.IsRegistered<PencuilState<TestRenderContext>>(), Is.True);
            Assert.That(builder.IsRegistered<PencilSystem<TestRenderContext>>(), Is.True);
        });
    }

    private sealed class TestRenderContext : IRenderContext
    {
        public CommandBuffer CommandBuffer => null!;
        public Texture ColorTarget => null!;

        public void Dispose()
        {
        }
    }
}
