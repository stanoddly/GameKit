namespace GameKit.Audio;

public interface IAudioSystem
{
    AudioListener Listener { get; }
    float MasterGain { get; set; }

    AudioBuffer LoadBuffer(ReadOnlySpan<char> path);
    AudioStream OpenStream(ReadOnlySpan<char> path);
    AudioSource CreateSource();
    AudioGroup CreateGroup(string name);
}
