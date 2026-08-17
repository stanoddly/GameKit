using GameKit.Input;

namespace GameKit.Tests;

public sealed class PriorityEventHandlersTests
{
    private static readonly ViewScope _firstView = new(1);
    private static readonly ViewScope _secondView = new(2);

    [Test]
    public void Invoke_OrdersHandlersByPriorityAndStopsWhenConsumed()
    {
        PriorityEventHandlers<string, TestEventArgs> handlers = new();
        List<string> calls = new();
        handlers.Add(10, (sender, _) => calls.Add($"late:{sender}"));
        handlers.Add(-10, (sender, _) => calls.Add($"early:{sender}"));
        handlers.Add(0, (sender, eventArgs) =>
        {
            calls.Add($"consume:{sender}");
            eventArgs.Consume();
        });

        handlers.Invoke("sender", new TestEventArgs());

        Assert.That(calls, Is.EqualTo(new[] { "early:sender", "consume:sender" }));
    }

    [Test]
    public void Invoke_InvokesOnlyHandlersForMatchingViewInPriorityOrder()
    {
        ViewScopedPriorityEventHandlers<string, TestEventArgs> handlers = new();
        List<string> calls = new();
        handlers.Add(_secondView, -10, (_, _) => calls.Add("second"));
        handlers.Add(_firstView, 10, (_, _) => calls.Add("late"));
        handlers.Add(_firstView, -10, (_, _) => calls.Add("early"));

        handlers.Invoke(_firstView, "sender", new TestEventArgs());

        Assert.That(calls, Is.EqualTo(new[] { "early", "late" }));
    }

    [Test]
    public void Invoke_ResetsConsumedBeforeDispatch()
    {
        ViewScopedPriorityEventHandlers<string, TestEventArgs> handlers = new();
        TestEventArgs eventArgs = new();
        bool called = false;
        handlers.Add(_firstView, 0, (_, _) => called = true);
        eventArgs.Consume();

        handlers.Invoke(_firstView, "sender", eventArgs);

        Assert.Multiple(() =>
        {
            Assert.That(called, Is.True);
            Assert.That(eventArgs.Consumed, Is.False);
        });
    }

    private sealed class TestEventArgs : ConsumableInputEventArgs
    {
    }
}
