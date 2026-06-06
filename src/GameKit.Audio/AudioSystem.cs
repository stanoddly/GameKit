using System.Text;
using GameKit.Content;
using GameKit.Common;
using GameKit.Utilities;
using SDL;

namespace GameKit.Audio;

public unsafe sealed class AudioSystem : IAudioSystem, IDisposable
{
    private unsafe delegate void Utf8Action(byte* value);

    private readonly GameKitFactory _sdlLifetime;
    private readonly VirtualFileSystem _fileSystem;
    private LockedSet<AudioSource> _sources = new();
    private LockedSet<AudioBuffer> _buffers = new();
    private Pointer<MIX_Mixer> _mixer;
    private float _masterGain = 1.0f;
    private bool _sdlAudioInitialized;
    private bool _mixerInitialized;

    internal AudioSystem(
        GameKitFactory sdlLifetime,
        VirtualFileSystem fileSystem,
        Pointer<MIX_Mixer> mixer,
        bool sdlAudioInitialized,
        bool mixerInitialized)
    {
        _sdlLifetime = sdlLifetime;
        _fileSystem = fileSystem;
        _mixer = mixer;
        _sdlAudioInitialized = sdlAudioInitialized;
        _mixerInitialized = mixerInitialized;

        Listener = new AudioListener(this);
        Groups = new AudioGroups(this);
    }

    public AudioListener Listener { get; }
    public AudioGroups Groups { get; }

    public float MasterGain
    {
        get
        {
            return _masterGain;
        }
        set
        {
            ThrowIfDisposed();
            ThrowIfNegative(value, nameof(value));
            ThrowIfSdlFailed(
                SDL3_mixer.MIX_SetMixerGain(_mixer, value),
                "MIX_SetMixerGain");
            _masterGain = value;
        }
    }

    public AudioBuffer LoadBuffer(ReadOnlySpan<char> path)
    {
        ThrowIfDisposed();

        using Stream fileStream = _fileSystem.OpenStream(path);
        using MemoryStream memoryStream = new();
        fileStream.CopyTo(memoryStream);
        byte[] fileData = memoryStream.ToArray();

        fixed (byte* fileDataPointer = fileData)
        {
            Pointer<SDL_IOStream> ioStream = SDL3.SDL_IOFromConstMem((IntPtr)fileDataPointer, (UIntPtr)fileData.Length);
            if (ioStream.IsNull)
            {
                throw new AudioException($"SDL_IOFromConstMem failed: {SDL3.SDL_GetError()}");
            }

            Pointer<MIX_Audio> sdlAudio = SDL3_mixer.MIX_LoadAudio_IO(_mixer, ioStream, true, true);
            if (sdlAudio.IsNull)
            {
                throw new AudioException($"MIX_LoadAudio_IO failed: {SDL3.SDL_GetError()}");
            }

            AudioBuffer buffer = new(this, sdlAudio);
            _buffers.Add(buffer);
            return buffer;
        }
    }

    public AudioSource CreateSource()
    {
        ThrowIfDisposed();

        Pointer<MIX_Track> track = SDL3_mixer.MIX_CreateTrack(_mixer);
        if (track.IsNull)
        {
            throw new AudioException($"MIX_CreateTrack failed: {SDL3.SDL_GetError()}");
        }

        AudioSource source = new(this, track);
        _sources.Add(source);
        return source;
    }

    public AudioGroup CreateGroup(string name)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Audio group name cannot be empty.", nameof(name));
        }

        return new AudioGroup(this, name);
    }

    public void Dispose()
    {
        if (_mixer.IsNull && !_mixerInitialized && !_sdlAudioInitialized)
        {
            return;
        }

        foreach (AudioSource source in _sources.ClearAndCopy())
        {
            ReleaseSource(source);
        }

        foreach (AudioBuffer buffer in _buffers.ClearAndCopy())
        {
            ReleaseBuffer(buffer);
        }

        if (!_mixer.IsNull)
        {
            SDL3_mixer.MIX_DestroyMixer(_mixer);
            _mixer = Pointer<MIX_Mixer>.Null;
        }

        if (_mixerInitialized)
        {
            SDL3_mixer.MIX_Quit();
            _mixerInitialized = false;
        }

        if (_sdlAudioInitialized)
        {
            SDL3.SDL_QuitSubSystem(SDL_InitFlags.SDL_INIT_AUDIO);
            _sdlAudioInitialized = false;
        }

        GC.KeepAlive(_sdlLifetime);
    }

    internal void SetSourceGroup(AudioSource source, AudioGroup? oldGroup, AudioGroup? newGroup)
    {
        if (oldGroup != null)
        {
            WithUtf8(oldGroup.Name, tag =>
            {
                SDL3_mixer.MIX_UntagTrack(source.SdlTrack, tag);
            });
        }

        if (newGroup != null)
        {
            WithUtf8(newGroup.Name, tag =>
            {
                ThrowIfSdlFailed(
                    SDL3_mixer.MIX_TagTrack(source.SdlTrack, tag),
                    "MIX_TagTrack");
            });
        }
    }

    internal void SetGroupGain(AudioGroup group, float gain)
    {
        ThrowIfDisposed();
        WithUtf8(group.Name, tag =>
        {
            ThrowIfSdlFailed(
                SDL3_mixer.MIX_SetTagGain(_mixer, tag, gain),
                "MIX_SetTagGain");
        });
    }

    internal void PauseGroup(AudioGroup group)
    {
        ThrowIfDisposed();
        WithUtf8(group.Name, tag =>
        {
            ThrowIfSdlFailed(
                SDL3_mixer.MIX_PauseTag(_mixer, tag),
                "MIX_PauseTag");
        });
    }

    internal void ResumeGroup(AudioGroup group)
    {
        ThrowIfDisposed();
        WithUtf8(group.Name, tag =>
        {
            ThrowIfSdlFailed(
                SDL3_mixer.MIX_ResumeTag(_mixer, tag),
                "MIX_ResumeTag");
        });
    }

    internal void StopGroup(AudioGroup group)
    {
        ThrowIfDisposed();
        WithUtf8(group.Name, tag =>
        {
            ThrowIfSdlFailed(
                SDL3_mixer.MIX_StopTag(_mixer, tag, 0),
                "MIX_StopTag");
        });
    }

    internal void UpdateSourcePositions()
    {
        foreach (AudioSource source in _sources.Copy())
        {
            source.ApplyPosition();
        }
    }

    internal void ReleaseSource(AudioSource source)
    {
        _sources.Remove(source);
        Pointer<MIX_Track> pointer = source.Pointer;
        if (pointer.IsNull)
        {
            return;
        }

        SDL3_mixer.MIX_DestroyTrack(pointer);
        source.Pointer = Pointer<MIX_Track>.Null;
    }

    internal void ReleaseBuffer(AudioBuffer buffer)
    {
        _buffers.Remove(buffer);
        Pointer<MIX_Audio> pointer = buffer.Pointer;
        if (pointer.IsNull)
        {
            return;
        }

        SDL3_mixer.MIX_DestroyAudio(pointer);
        buffer.Pointer = Pointer<MIX_Audio>.Null;
    }

    internal static void ThrowIfSdlFailed(SDLBool result, string operation)
    {
        if (result == false)
        {
            throw new AudioException($"{operation} failed: {SDL3.SDL_GetError()}");
        }
    }

    internal static void ThrowIfNegative(float value, string parameterName)
    {
        if (value < 0.0f)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Audio gain cannot be negative.");
        }
    }

    private static void WithUtf8(string value, Utf8Action action)
    {
        int byteCount = Encoding.UTF8.GetByteCount(value);
        byte[] bytes = new byte[byteCount + 1];
        Encoding.UTF8.GetBytes(value, bytes);
        fixed (byte* pointer = bytes)
        {
            action(pointer);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_mixer.IsNull)
        {
            throw new ObjectDisposedException(nameof(AudioSystem));
        }
    }
}
