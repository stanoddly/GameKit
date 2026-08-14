using GameKit.DependencyInjection;
using GameKit.Input;
using SDL;

namespace GameKit.Tests;

public class WindowEventSinkTests
{
    [Test]
    public void AddWindow_RegistersTypedWindowAndInputContracts()
    {
        ServiceCollection services = new();

        services.AddWindow<FirstWindow>(new WindowOptions());

        Assert.Multiple(() =>
        {
            Assert.That(services.IsRegistered<FirstWindow>(), Is.True);
            Assert.That(services.IsRegistered<IKeyboardService<FirstWindow>>(), Is.True);
            Assert.That(services.IsRegistered<IMouseService<FirstWindow>>(), Is.True);
            Assert.That(services.IsRegistered<ITextInputService<FirstWindow>>(), Is.True);
            Assert.That(services.IsRegistered<SecondWindow>(), Is.False);
        });
    }

    [Test]
    public void AddWindow_WithDuplicateIdentity_Throws()
    {
        ServiceCollection services = new();
        services.AddWindow<FirstWindow>(new WindowOptions());

        Assert.Throws<InvalidOperationException>(() =>
            services.AddWindow<FirstWindow>(new WindowOptions()));
    }

    [Test]
    public void Process_KeyEvent_NotifiesOnlyMatchingTypedInputService()
    {
        FirstWindow firstWindow = new();
        SecondWindow secondWindow = new();
        AppControl appControl = new();

        KeyboardService<FirstWindow> firstKeyboard = new(appControl);
        KeyboardService<SecondWindow> secondKeyboard = new(appControl);
        MouseService<FirstWindow> firstMouse = new(false);
        MouseService<SecondWindow> secondMouse = new(false);
        TextInputService<FirstWindow> firstTextInput = new(firstWindow);
        TextInputService<SecondWindow> secondTextInput = new(secondWindow);

        using WindowEventSink<FirstWindow> firstSink = new(
            firstWindow,
            firstKeyboard,
            firstMouse,
            firstTextInput);
        using WindowEventSink<SecondWindow> secondSink = new(
            secondWindow,
            secondKeyboard,
            secondMouse,
            secondTextInput);

        int firstCalls = 0;
        int secondCalls = 0;
        firstKeyboard.KeyDown += (_, _) => firstCalls++;
        secondKeyboard.KeyDown += (_, _) => secondCalls++;

        SDL_Event evt = default;
        evt.key.type = SDL_EventType.SDL_EVENT_KEY_DOWN;
        evt.key.down = true;
        evt.key.which = (SDL_KeyboardID)1;
        evt.key.scancode = SDL_Scancode.SDL_SCANCODE_A;

        firstSink.Process(in evt);

        Assert.That(firstCalls, Is.EqualTo(1));
        Assert.That(secondCalls, Is.EqualTo(0));
    }

    private sealed class FirstWindow : Window
    {
        public FirstWindow()
        {
        }
    }

    private sealed class SecondWindow : Window
    {
        public SecondWindow()
        {
        }
    }
}
