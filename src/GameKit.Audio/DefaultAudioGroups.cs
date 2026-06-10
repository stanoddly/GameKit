namespace GameKit.Audio;

public sealed record DefaultAudioGroups
{
    private DefaultAudioGroups(
        AudioGroup effects,
        AudioGroup music,
        AudioGroup ambience,
        AudioGroup ui)
    {
        Effects = effects;
        Music = music;
        Ambience = ambience;
        Ui = ui;
    }

    public AudioGroup Effects { get; }
    public AudioGroup Music { get; }
    public AudioGroup Ambience { get; }
    public AudioGroup Ui { get; }

    internal static DefaultAudioGroups Create(AudioSystem audioSystem)
    {
        return new DefaultAudioGroups(
            audioSystem.CreateGroup("effects"),
            audioSystem.CreateGroup("music"),
            audioSystem.CreateGroup("ambience"),
            audioSystem.CreateGroup("ui"));
    }
}
