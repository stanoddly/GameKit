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
            Pointer<MIX_Audio> sdlAudio = value == null ? Pointer<MIX_Audio>.Null : value.SdlAudio;
            AudioSystem.ThrowIfSdlFailed(
                SDL3_mixer.MIX_SetTrackAudio(SdlTrack, sdlAudio),
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
            AudioSystem.ThrowIfNegative(value, nameof(value));
            AudioSystem.ThrowIfSdlFailed(
                SDL3_mixer.MIX_SetTrackGain(SdlTrack, value),
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
            _position = value;
            ApplyPosition();
        }
    }

    public AudioSourceState State
    {
        get
        {
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

    public void Play()
    {
        if (_buffer == null)
        {
            throw new InvalidOperationException("Cannot play an audio source without a buffer.");
        }

        ApplyLooping();
        ApplyPosition();
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_PlayTrack(SdlTrack, 0),
            "MIX_PlayTrack");
    }

    public void Pause()
    {
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_PauseTrack(SdlTrack),
            "MIX_PauseTrack");
    }

    public void Resume()
    {
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_ResumeTrack(SdlTrack),
            "MIX_ResumeTrack");
    }

    public void Stop()
    {
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_StopTrack(SdlTrack, 0),
            "MIX_StopTrack");
    }

    public void Dispose()
    {
        _audioSystem.ReleaseSource(this);
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
            SDL3_mixer.MIX_SetTrack3DPosition(SdlTrack, &point),
            "MIX_SetTrack3DPosition");
    }

    private void ApplyLooping()
    {
        int loopCount = _looping ? -1 : 0;
        AudioSystem.ThrowIfSdlFailed(
            SDL3_mixer.MIX_SetTrackLoops(SdlTrack, loopCount),
            "MIX_SetTrackLoops");
    }
}
