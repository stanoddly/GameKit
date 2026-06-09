using GameKit.Architecture.Events;

namespace GameKit.Architecture.Tests;

public sealed record TestMessage(int Value) : DomainMessage;

[TestFixture]
public sealed class DomainEventStreamTests
{
    private const int MaximumRetainedEvents = 8192;

    [Test]
    public void Cursor_ReadsPublishedMessagesInOrder()
    {
        DomainEventStream stream = new();
        DomainEventCursor cursor = stream.CreateCursor();

        stream.Publish(new TestMessage(1));
        stream.Publish(new TestMessage(2));
        stream.Publish(new TestMessage(3));

        Assert.That(Drain(cursor), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(cursor.TryRead(out _), Is.False);
    }

    [Test]
    public void Cursor_OnlySeesMessagesPublishedAfterItsCreation()
    {
        DomainEventStream stream = new();
        stream.Publish(new TestMessage(1));

        DomainEventCursor cursor = stream.CreateCursor();
        stream.Publish(new TestMessage(2));

        Assert.That(Drain(cursor), Is.EqualTo(new[] { 2 }));
    }

    [Test]
    public void Cursors_DrainIndependentlyAtTheirOwnPace()
    {
        DomainEventStream stream = new();
        DomainEventCursor fast = stream.CreateCursor();
        DomainEventCursor slow = stream.CreateCursor();

        stream.Publish(new TestMessage(1));
        stream.Publish(new TestMessage(2));

        Assert.That(Drain(fast), Is.EqualTo(new[] { 1, 2 }));

        stream.Publish(new TestMessage(3));

        // The slow cursor still sees everything from where it started.
        Assert.That(Drain(slow), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(Drain(fast), Is.EqualTo(new[] { 3 }));
    }

    [Test]
    public void Buffer_GrowsBeyondInitialCapacityPreservingOrder()
    {
        DomainEventStream stream = new();
        DomainEventCursor cursor = stream.CreateCursor();

        // Far beyond the initial capacity of 16, without draining, forcing growth.
        int[] expected = Enumerable.Range(0, 100).ToArray();
        foreach (int value in expected)
        {
            stream.Publish(new TestMessage(value));
        }

        Assert.That(Drain(cursor), Is.EqualTo(expected));
    }

    [Test]
    public void Buffer_WrapsAroundWhenInterleavingPublishAndRead()
    {
        DomainEventStream stream = new();
        DomainEventCursor cursor = stream.CreateCursor();

        // Interleaving advances _head past the modulo boundary repeatedly.
        List<int> read = new();
        for (int i = 0; i < 100; i++)
        {
            stream.Publish(new TestMessage(i));
            Assert.That(cursor.TryRead(out DomainMessage? message), Is.True);
            read.Add(((TestMessage)message!).Value);
        }

        Assert.That(read, Is.EqualTo(Enumerable.Range(0, 100)));
    }

    [Test]
    public void Compaction_ReleasesEventsOnceEveryCursorHasConsumedThem()
    {
        DomainEventStream stream = new();
        DomainEventCursor cursor = stream.CreateCursor();

        // With a single cursor that keeps up, the buffer never overflows no
        // matter how many events flow through it.
        for (int i = 0; i < MaximumRetainedEvents * 3; i++)
        {
            stream.Publish(new TestMessage(i));
            cursor.TryRead(out _);
        }

        Assert.Pass();
    }

    [Test]
    public void Publish_ThrowsWhenAStalledCursorRetainsTooManyEvents()
    {
        DomainEventStream stream = new();
        // A cursor that never reads stalls compaction.
        stream.CreateCursor();

        for (int i = 0; i < MaximumRetainedEvents; i++)
        {
            stream.Publish(new TestMessage(i));
        }

        Assert.That(() => stream.Publish(new TestMessage(MaximumRetainedEvents)),
            Throws.InvalidOperationException);
    }

    [Test]
    public void DisposingStalledCursor_FreesTheBufferForCompaction()
    {
        DomainEventStream stream = new();
        DomainEventCursor stalled = stream.CreateCursor();

        for (int i = 0; i < MaximumRetainedEvents; i++)
        {
            stream.Publish(new TestMessage(i));
        }

        stalled.Dispose();

        // With no cursors retaining the backlog, publishing succeeds again.
        Assert.That(() => stream.Publish(new TestMessage(MaximumRetainedEvents)),
            Throws.Nothing);
    }

    [Test]
    public void DisposedCursor_ThrowsOnRead()
    {
        DomainEventStream stream = new();
        DomainEventCursor cursor = stream.CreateCursor();
        cursor.Dispose();

        Assert.That(() => cursor.TryRead(out _), Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Cursor_OnEmptyStreamReturnsFalse()
    {
        DomainEventStream stream = new();
        DomainEventCursor cursor = stream.CreateCursor();

        Assert.That(cursor.TryRead(out DomainMessage? message), Is.False);
        Assert.That(message, Is.Null);
    }

    private static int[] Drain(DomainEventCursor cursor)
    {
        List<int> values = new();
        while (cursor.TryRead(out DomainMessage? message))
        {
            values.Add(((TestMessage)message!).Value);
        }

        return values.ToArray();
    }
}
