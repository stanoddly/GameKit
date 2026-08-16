using System.Runtime.CompilerServices;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public class WindowRegistryTests
{
    [Test]
    public void Register_IndexesWindowByRenderContextAndSdlId()
    {
        WindowRegistry windowRegistry = new();
        Window<TestRenderContext> window =
            (Window<TestRenderContext>)RuntimeHelpers.GetUninitializedObject(
                typeof(Window<TestRenderContext>));

        windowRegistry.Register(window);

        Assert.Multiple(() =>
        {
            Assert.That(
                windowRegistry.TryGetWindow<TestRenderContext>(out Window<TestRenderContext> typedWindow),
                Is.True);
            Assert.That(typedWindow, Is.SameAs(window));
            Assert.That(windowRegistry.TryGetWindow(window.Id, out Window sdlWindow), Is.True);
            Assert.That(sdlWindow, Is.SameAs(window));
        });
    }

    [Test]
    public void Unregister_RemovesBothIndexes()
    {
        WindowRegistry windowRegistry = new();
        Window<TestRenderContext> window =
            (Window<TestRenderContext>)RuntimeHelpers.GetUninitializedObject(
                typeof(Window<TestRenderContext>));
        windowRegistry.Register(window);

        windowRegistry.Unregister(window);

        Assert.Multiple(() =>
        {
            Assert.That(
                windowRegistry.TryGetWindow<TestRenderContext>(out Window<TestRenderContext> _),
                Is.False);
            Assert.That(windowRegistry.TryGetWindow(window.Id, out Window _), Is.False);
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
