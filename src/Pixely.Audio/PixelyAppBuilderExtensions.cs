using Pixely.App;

namespace Pixely.Audio;

public static class PixelyAppBuilderExtensions
{
    public static PixelyAppBuilder RegisterAudio(this PixelyAppBuilder builder)
    {
        builder.AddSingleton<AudioFactory>();
        builder.AddSingleton<AudioSystem, AudioFactory>();
        builder.AddAlias<IAudioSystem, AudioSystem>();
        return builder;
    }
}
