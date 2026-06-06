using GameKit.Utilities;
using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioBuffer : IDisposable
{
    private readonly AudioSystem _audioSystem;
    internal Pointer<MIX_Audio> Pointer { get; set; }

    internal AudioBuffer(AudioSystem audioSystem, Pointer<MIX_Audio> sdlAudio)
    {
        _audioSystem = audioSystem;
        Pointer = sdlAudio;
    }

    internal Pointer<MIX_Audio> SdlAudio
    {
        get
        {
            Pointer.ThrowIfNull("Audio buffer has been disposed.");
            return Pointer;
        }
    }

    public void Dispose()
    {
        _audioSystem.ReleaseBuffer(this);
    }
}
