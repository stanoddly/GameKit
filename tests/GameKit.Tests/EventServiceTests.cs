using System.Runtime.CompilerServices;
using GameKit.DependencyInjection;
using GameKit.Input;

namespace GameKit.Tests;

public sealed class EventServiceTests
{
    [Test]
    public void WindowEventService_CreateAndDispose_AttachesAndDetaches()
    {
        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        WindowManager windowManager = new(provider);
        EventService eventService = new(new GamepadService(), new AppControl());
        Window window = CreateWindow(42);
        AppControl appControl = new();

        WindowEventService windowEvents = WindowEventService.Create(
            window,
            new KeyboardService(appControl),
            new MouseService(),
            new TextInputService(window),
            windowManager,
            appControl,
            eventService);

        Assert.That(
            eventService.TryGetWindowEventService(42, out WindowEventService attached),
            Is.True);
        Assert.That(attached, Is.SameAs(windowEvents));

        windowEvents.Dispose();

        Assert.That(eventService.TryGetWindowEventService(42, out _), Is.False);
    }

    [Test]
    public void TryGetWindowEventService_WithSparseWindowIds_FindsAttachedServices()
    {
        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        WindowManager windowManager = new(provider);
        EventService eventService = new(new GamepadService(), new AppControl());
        WindowEventService first = CreateWindowEventService(eventService, windowManager, 3);
        WindowEventService second = CreateWindowEventService(eventService, windowManager, 1_000_000_000);

        eventService.Attach(first);
        eventService.Attach(second);

        Assert.Multiple(() =>
        {
            Assert.That(
                eventService.TryGetWindowEventService(3, out WindowEventService foundFirst),
                Is.True);
            Assert.That(foundFirst, Is.SameAs(first));
            Assert.That(
                eventService.TryGetWindowEventService(
                    1_000_000_000,
                    out WindowEventService foundSecond),
                Is.True);
            Assert.That(foundSecond, Is.SameAs(second));
            Assert.That(eventService.TryGetWindowEventService(4, out _), Is.False);
        });
    }

    [Test]
    public void Detach_RemovesOnlyMatchingWindowEventService()
    {
        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        WindowManager windowManager = new(provider);
        EventService eventService = new(new GamepadService(), new AppControl());
        WindowEventService first = CreateWindowEventService(eventService, windowManager, 10);
        WindowEventService second = CreateWindowEventService(eventService, windowManager, 20);
        WindowEventService third = CreateWindowEventService(eventService, windowManager, 30);
        eventService.Attach(first);
        eventService.Attach(second);
        eventService.Attach(third);

        eventService.Detach(second);

        Assert.Multiple(() =>
        {
            Assert.That(
                eventService.TryGetWindowEventService(10, out WindowEventService foundFirst),
                Is.True);
            Assert.That(foundFirst, Is.SameAs(first));
            Assert.That(eventService.TryGetWindowEventService(20, out _), Is.False);
            Assert.That(
                eventService.TryGetWindowEventService(30, out WindowEventService foundThird),
                Is.True);
            Assert.That(foundThird, Is.SameAs(third));
        });
    }

    [Test]
    public void Attach_WithDuplicateWindowId_Throws()
    {
        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        WindowManager windowManager = new(provider);
        EventService eventService = new(new GamepadService(), new AppControl());
        WindowEventService first = CreateWindowEventService(eventService, windowManager, 10);
        WindowEventService second = CreateWindowEventService(eventService, windowManager, 10);
        eventService.Attach(first);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            eventService.Attach(second))!;

        Assert.That(exception.Message, Does.Contain("10"));
    }

    [Test]
    public void WindowManager_AttachAndDetach_TracksWindowOwnership()
    {
        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        WindowManager windowManager = new(provider);
        EventService eventService = new(new GamepadService(), new AppControl());
        WindowEventService windowEvents = CreateWindowEventService(eventService, windowManager, 10);

        windowManager.Attach(windowEvents.Window, provider);

        Assert.That(windowManager.Windows, Is.EqualTo(new[] { windowEvents.Window }));

        windowManager.Detach(windowEvents.Window);

        Assert.That(windowManager.Windows, Is.Empty);
    }

    private static WindowEventService CreateWindowEventService(
        EventService eventService,
        WindowManager windowManager,
        uint windowId)
    {
        Window window = CreateWindow(windowId);
        AppControl appControl = new();

        return new WindowEventService(
            window,
            new KeyboardService(appControl),
            new MouseService(),
            new TextInputService(window),
            windowManager,
            appControl,
            eventService);
    }

    private static Window CreateWindow(uint windowId)
    {
        ActivationWindow activation = new(default, default, windowId);
        Window window = (Window)RuntimeHelpers.GetUninitializedObject(typeof(Window));
        window.Activation = activation;
        return window;
    }
}
