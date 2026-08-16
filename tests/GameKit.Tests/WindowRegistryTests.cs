using System.Runtime.CompilerServices;
using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tests;

public class WindowRegistryTests
{
    [Test]
    public void Register_IndexesWindowByRenderContextAndSdlId()
    {
        WindowRegistry windows = new();
        Window<TestRenderContext> window =
            (Window<TestRenderContext>)RuntimeHelpers.GetUninitializedObject(
                typeof(Window<TestRenderContext>));

        windows.Register(window);

        Assert.Multiple(() =>
        {
            Assert.That(
                windows.TryGetWindow<TestRenderContext>(out Window<TestRenderContext> typedWindow),
                Is.True);
            Assert.That(typedWindow, Is.SameAs(window));
            Assert.That(windows.TryGetWindow(window.Id, out Window sdlWindow), Is.True);
            Assert.That(sdlWindow, Is.SameAs(window));
        });
    }

    [Test]
    public void Unregister_RemovesBothIndexes()
    {
        WindowRegistry windows = new();
        Window<TestRenderContext> window =
            (Window<TestRenderContext>)RuntimeHelpers.GetUninitializedObject(
                typeof(Window<TestRenderContext>));
        windows.Register(window);

        windows.Unregister(window);

        Assert.Multiple(() =>
        {
            Assert.That(
                windows.TryGetWindow<TestRenderContext>(out Window<TestRenderContext> _),
                Is.False);
            Assert.That(windows.TryGetWindow(window.Id, out Window _), Is.False);
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
