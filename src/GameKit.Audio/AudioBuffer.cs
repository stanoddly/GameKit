using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioBuffer : IDisposable
{
    private readonly AudioSystem _audioSystem;
    private MIX_Audio* _sdlAudio;
    private bool _disposed;

    internal AudioBuffer(AudioSystem audioSystem, MIX_Audio* sdlAudio)
    {
        _audioSystem = audioSystem;
        _sdlAudio = sdlAudio;
    }

    internal MIX_Audio* SdlAudio
    {
        get
        {
            ThrowIfDisposed();
            return _sdlAudio;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_sdlAudio != null)
        {
            SDL3_mixer.MIX_DestroyAudio(_sdlAudio);
            _sdlAudio = null;
        }

        _audioSystem.Untrack(this);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
