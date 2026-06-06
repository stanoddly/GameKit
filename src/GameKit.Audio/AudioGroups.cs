namespace GameKit.Audio;

public sealed class AudioGroups
{
    internal AudioGroups(AudioSystem audioSystem)
    {
        Effects = audioSystem.CreateGroup("effects");
        Music = audioSystem.CreateGroup("music");
        Ambience = audioSystem.CreateGroup("ambience");
        Ui = audioSystem.CreateGroup("ui");
    }

    public AudioGroup Effects { get; }
    public AudioGroup Music { get; }
    public AudioGroup Ambience { get; }
    public AudioGroup Ui { get; }
}
