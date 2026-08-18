namespace Pixely.Architecture.Events;

public sealed class DomainEventStream : IDomainEventPublisher, IDomainEventStream
{
    private const int InitialCapacity = 16;
    private const int MaximumRetainedEvents = 8192;

    private DomainMessage?[] _messages = new DomainMessage[InitialCapacity];
    private readonly List<DomainEventCursor> _cursors = new();
    private int _head;
    private int _count;
    private long _firstSequence;
    private long _nextSequence;

    public void Publish(DomainMessage domainMessage)
    {
        if (_count == MaximumRetainedEvents)
        {
            throw new InvalidOperationException("Domain event stream retained too many events. A cursor may be stalled.");
        }

        EnsureCapacity(_count + 1);
        int messageIndex = PhysicalIndex(_count);
        _messages[messageIndex] = domainMessage;
        _count++;
        _nextSequence++;
        Compact();
    }

    public DomainEventCursor CreateCursor()
    {
        DomainEventCursor cursor = new DomainEventCursor(this, _nextSequence);
        _cursors.Add(cursor);
        return cursor;
    }

    internal bool TryRead(DomainEventCursor cursor, out DomainMessage? domainMessage)
    {
        if (_count == 0)
        {
            domainMessage = null;
            return false;
        }

        if (cursor.NextSequence < _firstSequence)
        {
            throw new InvalidOperationException("Domain event cursor lagged behind the retained event buffer.");
        }

        int offset = checked((int)(cursor.NextSequence - _firstSequence));
        if (offset >= _count)
        {
            domainMessage = null;
            return false;
        }

        domainMessage = _messages[PhysicalIndex(offset)]!;
        cursor.Advance();
        Compact();
        return true;
    }

    internal void RemoveCursor(DomainEventCursor cursor)
    {
        _cursors.Remove(cursor);
        Compact();
    }

    private void Compact()
    {
        if (_count == 0)
        {
            return;
        }

        long minimumNextSequence = _cursors.Count == 0
            ? _nextSequence
            : _cursors.Min(cursor => cursor.NextSequence);

        int removeCount = checked((int)(minimumNextSequence - _firstSequence));
        if (removeCount > 0)
        {
            for (int i = 0; i < removeCount; i++)
            {
                _messages[PhysicalIndex(i)] = null;
            }

            _head = PhysicalIndex(removeCount);
            _count -= removeCount;
            _firstSequence += removeCount;
        }
    }

    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _messages.Length)
        {
            return;
        }

        int newCapacity = _messages.Length * 2;
        while (newCapacity < requiredCapacity)
        {
            newCapacity *= 2;
        }

        if (newCapacity > MaximumRetainedEvents)
        {
            newCapacity = MaximumRetainedEvents;
        }

        DomainMessage?[] newMessages = new DomainMessage[newCapacity];
        for (int i = 0; i < _count; i++)
        {
            newMessages[i] = _messages[PhysicalIndex(i)];
        }

        _messages = newMessages;
        _head = 0;
    }

    private int PhysicalIndex(int offset)
    {
        return (_head + offset) % _messages.Length;
    }
}
