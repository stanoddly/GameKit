using System.Numerics;
using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioSource : IDisposable
{
    private readonly AudioSystem _audioSystem;
    private MIX_Track* _track;
    private AudioBuffer? _buffer;
    private AudioGroup? _group;
    private Vector3 _position;
    private float _gain = 1.0f;
    private bool _looping;
    private bool _disposed;

    internal AudioSource(AudioSystem audioSystem, MIX_Track* track)
    {
        _audioSystem = audioSystem;
        _track = track;
    }

    public AudioBuffer? Buffer
    {
        get
        {
            return _buffer;
        }
        set
        {
            ThrowIfDisposed();
            MIX_Audio* sdlAudio = value == null ? null : value.SdlAudio;
            AudioSystem.ThrowIfSdlFailed(
                SDL3_mixer.MIX_SetTrackAudio(_track, sdlAudio),
                "MIX_SetTrackAudio");
            _buffer = value;
        }
    }

    public AudioGroup? Group
    {
        get
        {
            return _group;
        }
        set
        {
            ThrowIfDisposed();
            _audioSystem.SetSourceGroup(this, _group, value);
            _group = value;
        }
    }

    public float Gain
    {
        get
        {
            return _gain;
        }
        set
        {
            ThrowIfDisposed();
            AudioSystem.ThrowIfNegative(value, nameof(value));
            AudioSystem.ThrowIfSdlFailed(
                SDL3_mixer.MIX_SetTrackGain(_track, value),
                "MIX_SetTrackGain");
            _gain = value;
        }
    }

    public bool Looping
    {
        get
        {
            return _looping;
        }
        set
        {
            ThrowIfDisposed();
            _looping = value;
            ApplyLooping();
        }
    }

    public Vector3 Position
    {
        get
        {
            return _position;
        }
        set
        {
            ThrowIfDisposed();
            _position = value;
            ApplyPosition();
        }
    }

    public AudioSourceState State
    {
        get
        {
            ThrowIfDisposed();
            if (SDL3_mixer.MIX_TrackPaused(_track))
            {
                return AudioSourceState.Paused;
            }

            if (SDL3_mixer.MIX_TrackPlaying(_track))
            {
                return AudioSourceState.Playing;
            }

            return AudioSourceState.Stopped;
        }
    }

    internal MIX_Track* SdlTrack
    {
        get
        {
            ThrowIfDisposed();
            return _track;
        }
    }

    public void Play()
    {
        ThrowIfDisposed();
        if (_buffer == null)
        {
            throw new InvalidOperationException("Cannot play an audio source without a buffer.");
        }

        ApplyLooping();
        ApplyPosition();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_PlayTrack(_track, 0),
            "MIX_PlayTrack");
    }

    public void Pause()
    {
        ThrowIfDisposed();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_PauseTrack(_track),
            "MIX_PauseTrack");
    }

    public void Resume()
    {
        ThrowIfDisposed();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_ResumeTrack(_track),
            "MIX_ResumeTrack");
    }

    public void Stop()
    {
        ThrowIfDisposed();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_StopTrack(_track, 0),
            "MIX_StopTrack");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_track != null)
        {
            SDL3_mixer.MIX_DestroyTrack(_track);
            _track = null;
        }

        _audioSystem.Untrack(this);
    }

    internal void ApplyPosition()
    {
        Vector3 relativePosition = _position - _audioSystem.Listener.Position;
        MIX_Point3D point = new()
        {
            x = relativePosition.X,
            y = relativePosition.Y,
            z = relativePosition.Z
        };

        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_SetTrack3DPosition(_track, &point),
            "MIX_SetTrack3DPosition");
    }

    private void ApplyLooping()
    {
        int loopCount = _looping ? -1 : 0;
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_SetTrackLoops(_track, loopCount),
            "MIX_SetTrackLoops");
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
