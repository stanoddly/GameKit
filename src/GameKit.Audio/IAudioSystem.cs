namespace GameKit.Audio;

public interface IAudioSystem
{
    AudioListener Listener { get; }
    DefaultAudioGroups Groups { get; }
    float MasterGain { get; set; }

    AudioBuffer LoadBuffer(ReadOnlySpan<char> path);
    AudioSource CreateSource();
    AudioGroup CreateGroup(string name);
}
