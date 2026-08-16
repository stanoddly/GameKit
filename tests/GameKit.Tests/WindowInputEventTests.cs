using System.Runtime.CompilerServices;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;
using SDL;

namespace GameKit.Tests;

public class WindowInputEventTests
{
    [Test]
    public void OnTextInputEvent_ProvidesSourceWindow()
    {
        TextInputService textInputService = new();
        Window<TestRenderContext> window =
            (Window<TestRenderContext>)RuntimeHelpers.GetUninitializedObject(
                typeof(Window<TestRenderContext>));
        TextInputEventArgs? receivedEventArgs = null;
        textInputService.TextInput += eventArgs => receivedEventArgs = eventArgs;
        SDL_TextInputEvent textInputEvent = new() { timestamp = 42 };

        textInputService.OnTextInputEvent(window, textInputEvent);

        Assert.Multiple(() =>
        {
            Assert.That(receivedEventArgs, Is.Not.Null);
            Assert.That(receivedEventArgs!.Window, Is.SameAs(window));
            Assert.That(receivedEventArgs.Timestamp, Is.EqualTo(42));
        });
    }

    [Test]
    public void OnMouseMotionEvent_ProvidesSourceWindow()
    {
        MouseService mouseService = new();
        Window<TestRenderContext> window =
            (Window<TestRenderContext>)RuntimeHelpers.GetUninitializedObject(
                typeof(Window<TestRenderContext>));
        MouseMotionEventArgs? receivedEventArgs = null;
        mouseService.Motion += (_, eventArgs) => receivedEventArgs = eventArgs;
        SDL_MouseMotionEvent mouseMotionEvent = new() { timestamp = 42 };

        mouseService.OnMouseMotionEvent(window, mouseMotionEvent);

        Assert.Multiple(() =>
        {
            Assert.That(receivedEventArgs, Is.Not.Null);
            Assert.That(receivedEventArgs!.Window, Is.SameAs(window));
            Assert.That(receivedEventArgs.Timestamp, Is.EqualTo(42));
        });
    }

    [Test]
    public void OnKeyEvent_ProvidesSourceWindow()
    {
        KeyboardService keyboardService = new(new AppControl());
        Window<TestRenderContext> window =
            (Window<TestRenderContext>)RuntimeHelpers.GetUninitializedObject(
                typeof(Window<TestRenderContext>));
        KeyEventArgs? receivedEventArgs = null;
        keyboardService.KeyDown += (_, eventArgs) => receivedEventArgs = eventArgs;
        SDL_KeyboardEvent keyboardEvent = new() { down = true, timestamp = 42 };

        keyboardService.OnKeyEvent(window, keyboardEvent);

        Assert.Multiple(() =>
        {
            Assert.That(receivedEventArgs, Is.Not.Null);
            Assert.That(receivedEventArgs!.Window, Is.SameAs(window));
            Assert.That(receivedEventArgs.Timestamp, Is.EqualTo(42));
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
