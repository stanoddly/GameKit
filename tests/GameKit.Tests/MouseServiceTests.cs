using GameKit.Input;
using SDL;

namespace GameKit.Tests;

public class MouseServiceTests
{
    [Test]
    public void WindowPresenceEventsUpdateStateAndIncludeTimestamp()
    {
        MouseService mouseService = new();
        List<string> events = new();

        mouseService.WindowEnter += eventArgs =>
        {
            events.Add($"enter:{eventArgs.IsInWindow}:{eventArgs.Timestamp}");
        };

        mouseService.WindowLeave += eventArgs =>
        {
            events.Add($"leave:{eventArgs.IsInWindow}:{eventArgs.Timestamp}");
        };

        SDL_WindowEvent enterEvent = new()
        {
            timestamp = 10
        };

        SDL_WindowEvent leaveEvent = new()
        {
            timestamp = 20
        };

        mouseService.OnMouseWindowPresenceEvent(enterEvent, true);

        Assert.That(mouseService.IsInWindow, Is.True);

        mouseService.OnMouseWindowPresenceEvent(leaveEvent, false);

        Assert.That(mouseService.IsInWindow, Is.False);
        Assert.That(events, Is.EqualTo(new[]
        {
            "enter:True:10",
            "leave:False:20"
        }));
    }

    [Test]
    public void WindowPresenceSubscriptionsRespectPriority()
    {
        MouseService mouseService = new();
        List<int> calls = new();

        mouseService.SubscribeWindowEnter(10, _ => calls.Add(10));
        mouseService.SubscribeWindowEnter(-10, _ => calls.Add(-10));

        SDL_WindowEvent windowEvent = new()
        {
            timestamp = 1
        };

        mouseService.OnMouseWindowPresenceEvent(windowEvent, true);

        Assert.That(calls, Is.EqualTo(new[]
        {
            -10,
            10
        }));
    }
}
