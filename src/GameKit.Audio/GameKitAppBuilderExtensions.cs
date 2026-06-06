using GameKit.App;

namespace GameKit.Audio;

public static class GameKitAppBuilderExtensions
{
    public static GameKitAppBuilder RegisterAudio(this GameKitAppBuilder builder)
    {
        builder.AddSingleton<AudioFactory>();
        builder.AddSingleton<AudioSystem, AudioFactory>();
        builder.AddAlias<IAudioSystem, AudioSystem>();
        return builder;
    }
}
