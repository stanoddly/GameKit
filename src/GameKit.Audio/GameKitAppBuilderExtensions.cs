using GameKit.App;

namespace GameKit.Audio;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder RegisterAudio(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<AudioSystem>();
        builder.AddAlias<IAudioSystem, AudioSystem>();
        return builder;
    }
}
