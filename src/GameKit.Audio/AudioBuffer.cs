using GameKit.Utilities;
using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioBuffer : IDisposable
{
    private readonly AudioSystem _audioSystem;
    internal Pointer<MIX_Audio> Pointer { get; set; }
    private bool _disposed;

    internal AudioBuffer(AudioSystem audioSystem, Pointer<MIX_Audio> sdlAudio)
    {
        _audioSystem = audioSystem;
        Pointer = sdlAudio;
    }

    internal Pointer<MIX_Audio> SdlAudio
    {
        get
        {
            ThrowIfDisposed();
            return Pointer;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _audioSystem.ReleaseBuffer(this);
    }

    internal void MarkDisposed()
    {
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
