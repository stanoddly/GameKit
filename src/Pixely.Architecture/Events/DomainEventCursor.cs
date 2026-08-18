using System.Diagnostics.CodeAnalysis;

namespace Pixely.Architecture.Events;

public sealed class DomainEventCursor : IDisposable
{
    private readonly DomainEventStream _stream;
    private bool _disposed;

    internal DomainEventCursor(DomainEventStream stream, long nextSequence)
    {
        _stream = stream;
        NextSequence = nextSequence;
    }

    internal long NextSequence { get; private set; }

    public bool TryRead([NotNullWhen(true)] out DomainMessage? domainMessage)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _stream.TryRead(this, out domainMessage);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _stream.RemoveCursor(this);
        _disposed = true;
    }

    internal void Advance()
    {
        NextSequence++;
    }
}
