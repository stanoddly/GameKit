using GameKit.Utilities;
using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioBuffer : AudioClip
{
    private readonly AudioSystem _audioSystem;
    internal Pointer<MIX_Audio> Pointer { get; set; }

    internal AudioBuffer(AudioSystem audioSystem, Pointer<MIX_Audio> sdlAudio)
    {
        _audioSystem = audioSystem;
        Pointer = sdlAudio;
    }

    internal override AudioSystem AudioSystem => _audioSystem;

    internal Pointer<MIX_Audio> SdlAudio
    {
        get
        {
            ThrowIfDisposed();
            return Pointer;
        }
    }

    internal override void AttachTo(AudioSource source)
    {
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_SetTrackAudio(source.SdlTrack, SdlAudio),
            nameof(SDL3_mixer.MIX_SetTrackAudio));
    }

    internal override void DetachFrom(AudioSource source)
    {
        SdlError.ThrowOnFalse(
            SDL3_mixer.MIX_SetTrackAudio(source.SdlTrack, Pointer<MIX_Audio>.Null),
            nameof(SDL3_mixer.MIX_SetTrackAudio));
    }

    public override void Dispose()
    {
        _audioSystem.ReleaseBuffer(this);
    }

    private void ThrowIfDisposed()
    {
        if (Pointer.IsNull)
        {
            throw new ObjectDisposedException(nameof(AudioBuffer));
        }
    }
}
