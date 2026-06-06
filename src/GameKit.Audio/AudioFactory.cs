using GameKit.Content;
using GameKit.Utilities;
using SDL;

namespace GameKit.Audio;

internal unsafe sealed class AudioFactory
{
    private readonly GameKitFactory _sdlLifetime;

    public AudioFactory(GameKitFactory sdlLifetime)
    {
        _sdlLifetime = sdlLifetime;
    }

    public AudioSystem CreateAudioSystem(VirtualFileSystem fileSystem)
    {
        bool sdlAudioInitialized = false;
        bool mixerInitialized = false;
        Pointer<MIX_Mixer> mixer = Pointer<MIX_Mixer>.Null;

        try
        {
            if (SDL3.SDL_InitSubSystem(SDL_InitFlags.SDL_INIT_AUDIO) == false)
            {
                throw new AudioException($"SDL_InitSubSystem(SDL_INIT_AUDIO) failed: {SDL3.SDL_GetError()}");
            }

            sdlAudioInitialized = true;

            if (SDL3_mixer.MIX_Init() == false)
            {
                throw new AudioException($"MIX_Init failed: {SDL3.SDL_GetError()}");
            }

            mixerInitialized = true;
            mixer = SDL3_mixer.MIX_CreateMixerDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, null);
            if (mixer.IsNull)
            {
                throw new AudioException($"MIX_CreateMixerDevice failed: {SDL3.SDL_GetError()}");
            }

            return new AudioSystem(_sdlLifetime, fileSystem, mixer, sdlAudioInitialized, mixerInitialized);
        }
        catch
        {
            if (!mixer.IsNull)
            {
                SDL3_mixer.MIX_DestroyMixer(mixer);
            }

            if (mixerInitialized)
            {
                SDL3_mixer.MIX_Quit();
            }

            if (sdlAudioInitialized)
            {
                SDL3.SDL_QuitSubSystem(SDL_InitFlags.SDL_INIT_AUDIO);
            }

            throw;
        }
    }
}
