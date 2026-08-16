using System.Runtime.CompilerServices;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.RenderOrchestration;
using SDL;

namespace GameKit.Tests;

public class TextInputServiceTests
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

    private sealed class TestRenderContext : IRenderContext
    {
        public CommandBuffer CommandBuffer => null!;
        public Texture ColorTarget => null!;

        public void Dispose()
        {
        }
    }
}
