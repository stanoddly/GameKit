using System.Numerics;
using GameKit.Utilities;
using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioSource : IDisposable
{
    private readonly AudioSystem _audioSystem;
    internal Pointer<MIX_Track> Pointer { get; set; }
    private AudioBuffer? _buffer;
    private AudioGroup? _group;
    private Vector3 _position;
    private float _gain = 1.0f;
    private bool _looping;
    private bool _disposed;

    internal AudioSource(AudioSystem audioSystem, Pointer<MIX_Track> track)
    {
        _audioSystem = audioSystem;
        Pointer = track;
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
            Pointer<MIX_Audio> sdlAudio = value == null ? Pointer<MIX_Audio>.Null : value.SdlAudio;
            AudioSystem.ThrowIfSdlFailed(
                SDL3_mixer.MIX_SetTrackAudio(Pointer, sdlAudio),
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
                SDL3_mixer.MIX_SetTrackGain(Pointer, value),
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
            if (SDL3_mixer.MIX_TrackPaused(Pointer))
            {
                return AudioSourceState.Paused;
            }

            if (SDL3_mixer.MIX_TrackPlaying(Pointer))
            {
                return AudioSourceState.Playing;
            }

            return AudioSourceState.Stopped;
        }
    }

    internal Pointer<MIX_Track> SdlTrack
    {
        get
        {
            ThrowIfDisposed();
            return Pointer;
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
            SDL3_mixer.MIX_PlayTrack(Pointer, 0),
            "MIX_PlayTrack");
    }

    public void Pause()
    {
        ThrowIfDisposed();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_PauseTrack(Pointer),
            "MIX_PauseTrack");
    }

    public void Resume()
    {
        ThrowIfDisposed();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_ResumeTrack(Pointer),
            "MIX_ResumeTrack");
    }

    public void Stop()
    {
        ThrowIfDisposed();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_StopTrack(Pointer, 0),
            "MIX_StopTrack");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _audioSystem.ReleaseSource(this);
    }

    internal void MarkDisposed()
    {
        _disposed = true;
        _buffer = null;
        _group = null;
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
            SDL3_mixer.MIX_SetTrack3DPosition(Pointer, &point),
            "MIX_SetTrack3DPosition");
    }

    private void ApplyLooping()
    {
        int loopCount = _looping ? -1 : 0;
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_SetTrackLoops(Pointer, loopCount),
            "MIX_SetTrackLoops");
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
