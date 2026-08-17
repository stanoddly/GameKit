using GameKit.Input;
using SDL;

namespace GameKit.Tests;

public sealed class ViewScopeInputEventTests
{
    private static readonly ViewScope _firstView = new(1);
    private static readonly ViewScope _secondView = new(2);

    [Test]
    public void KeyDown_ScopedHandlerReceivesOnlyMatchingView()
    {
        KeyboardService keyboardService = new(new AppControl());
        int defaultCalls = 0;
        int firstCalls = 0;
        int secondCalls = 0;
        Keyboard? eventKeyboard = null;
        keyboardService.KeyDown += _ => defaultCalls++;
        keyboardService.SubscribeKeyDown(_firstView, 0, eventArgs =>
        {
            firstCalls++;
            eventKeyboard = eventArgs.Keyboard;
        });
        keyboardService.SubscribeKeyDown(_secondView, 0, _ => secondCalls++);
        SDL_KeyboardEvent keyboardEvent = new() { down = true, timestamp = 42 };

        keyboardService.OnKeyEvent(_firstView, keyboardEvent);

        Assert.Multiple(() =>
        {
            Assert.That(defaultCalls, Is.Zero);
            Assert.That(firstCalls, Is.EqualTo(1));
            Assert.That(secondCalls, Is.Zero);
            Assert.That(eventKeyboard, Is.Not.Null);
        });
    }

    [Test]
    public void MouseMotion_ScopedHandlerReceivesOnlyMatchingView()
    {
        MouseService mouseService = new(new WindowRegistry());
        int defaultCalls = 0;
        int firstCalls = 0;
        int secondCalls = 0;
        Mouse? eventMouse = null;
        mouseService.Motion += _ => defaultCalls++;
        mouseService.SubscribeMotion(_firstView, 0, eventArgs =>
        {
            firstCalls++;
            eventMouse = eventArgs.Mouse;
        });
        mouseService.SubscribeMotion(_secondView, 0, _ => secondCalls++);
        SDL_MouseMotionEvent mouseMotionEvent = new() { timestamp = 42 };

        mouseService.OnMouseMotionEvent(_firstView, mouseMotionEvent);
        mouseService.OnMouseMotionEvent(default, mouseMotionEvent);

        Assert.Multiple(() =>
        {
            Assert.That(defaultCalls, Is.EqualTo(1));
            Assert.That(firstCalls, Is.EqualTo(1));
            Assert.That(secondCalls, Is.Zero);
            Assert.That(eventMouse, Is.Not.Null);
        });
    }

    [Test]
    public void TextInput_ScopedHandlerReceivesOnlyMatchingView()
    {
        TextInputService textInputService = new(new WindowRegistry());
        int defaultCalls = 0;
        int firstCalls = 0;
        int secondCalls = 0;
        textInputService.TextInput += _ => defaultCalls++;
        textInputService.SubscribeTextInput(_firstView, 0, _ => firstCalls++);
        textInputService.SubscribeTextInput(_secondView, 0, _ => secondCalls++);
        SDL_TextInputEvent textInputEvent = new() { timestamp = 42 };

        textInputService.OnTextInputEvent(_firstView, textInputEvent);
        textInputService.OnTextInputEvent(default, textInputEvent);

        Assert.Multiple(() =>
        {
            Assert.That(defaultCalls, Is.EqualTo(1));
            Assert.That(firstCalls, Is.EqualTo(1));
            Assert.That(secondCalls, Is.Zero);
        });
    }
}
