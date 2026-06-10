using System.Numerics;
using GameKit.Utilities;
using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioSource : IDisposable
{
    private readonly AudioSystem _audioSystem;
    internal Pointer<MIX_Track> Pointer { get; set; }
    private AudioClip? _clip;
    private AudioGroup? _group;
    private Vector3 _position;
    private float _gain = 1.0f;
    private bool _looping;

    internal AudioSource(AudioSystem audioSystem, Pointer<MIX_Track> track)
    {
        _audioSystem = audioSystem;
        Pointer = track;
    }

    public IAudioClip? Clip
    {
        get
        {
            return _clip;
        }
        set
        {
            ThrowIfDisposed();

            if (value is not null and not AudioClip)
            {
                throw new ArgumentException("Audio clip must be created by this audio system.", nameof(value));
            }

            AudioClip? newClip = (AudioClip?)value;
            if (newClip != null && !ReferenceEquals(newClip.AudioSystem, _audioSystem))
            {
                throw new ArgumentException("Audio clip must be created by this audio system.", nameof(value));
            }

            if (ReferenceEquals(_clip, newClip))
            {
                return;
            }

            _clip?.DetachFrom(this);
            _clip = null;
            newClip?.AttachTo(this);
            _clip = newClip;
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
            SdlError.ThrowOnFalse(
                SDL3_mixer.MIX_SetTrackGain(SdlTrack, value),
                nameof(SDL3_mixer.MIX_SetTrackGain));
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
            if (State != AudioSourceState.Stopped)
            {
                ApplyPosition();
            }
        }
    }

    public AudioSourceState State
    {
        get
        {
            ThrowIfDisposed();
            if (SDL3_mixer.MIX_TrackPaused(SdlTrack))
            {
                return AudioSourceState.Paused;
            }

            if (SDL3_mixer.MIX_TrackPlaying(SdlTrack))
            {
                return AudioSourceState.Playing;
            }

            return AudioSourceState.Stopped;
        }
    }

    internal Pointer<MIX_Track> SdlTrack => Pointer;

    internal void SetTrackAudio(Pointer<MIX_Audio> sdlAudio)
    {
        ThrowIfDisposed();
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_SetTrackAudio(SdlTrack, sdlAudio),
            nameof(SDL3_mixer.MIX_SetTrackAudio));
    }

    public void Play()
    {
        ThrowIfDisposed();
        if (_clip == null)
        {
            throw new InvalidOperationException("Cannot play an audio source without a clip.");
        }

        ApplyLooping();
        ApplyPosition();
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_PlayTrack(SdlTrack, 0),
            nameof(SDL3_mixer.MIX_PlayTrack));
    }

    public void Pause()
    {
        ThrowIfDisposed();
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_PauseTrack(SdlTrack),
            nameof(SDL3_mixer.MIX_PauseTrack));
    }

    public void Resume()
    {
        ThrowIfDisposed();
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_ResumeTrack(SdlTrack),
            nameof(SDL3_mixer.MIX_ResumeTrack));
    }

    public void Stop()
    {
        ThrowIfDisposed();
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_StopTrack(SdlTrack, 0),
            nameof(SDL3_mixer.MIX_StopTrack));
    }

    public void Dispose()
    {
        _audioSystem.ReleaseSource(this);
    }

    internal void ApplyPosition()
    {
        ThrowIfDisposed();
        Vector3 relativePosition = _position - _audioSystem.Listener.Position;
        MIX_Point3D point = new()
        {
            x = relativePosition.X,
            y = relativePosition.Y,
            z = relativePosition.Z
        };

        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_SetTrack3DPosition(SdlTrack, &point),
            nameof(SDL3_mixer.MIX_SetTrack3DPosition));
    }

    internal void ThrowIfDisposed()
    {
        if (Pointer.IsNull)
        {
            throw new ObjectDisposedException(nameof(AudioSource));
        }
    }

    private void ApplyLooping()
    {
        ThrowIfDisposed();
        int loopCount = _looping ? -1 : 0;
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_SetTrackLoops(SdlTrack, loopCount),
            nameof(SDL3_mixer.MIX_SetTrackLoops));
    }
}
