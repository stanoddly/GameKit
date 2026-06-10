using System.Text;

namespace GameKit.Audio;

public sealed class AudioGroup
{
    private readonly AudioSystem _audioSystem;
    private float _gain = 1.0f;

    internal AudioGroup(AudioSystem audioSystem, string name)
    {
        _audioSystem = audioSystem;
        Name = name;
        Utf8Name = new byte[Encoding.UTF8.GetByteCount(name) + 1];
        Encoding.UTF8.GetBytes(name, Utf8Name);
    }

    public string Name { get; }
    internal byte[] Utf8Name { get; }

    public float Gain
    {
        get
        {
            return _gain;
        }
        set
        {
            AudioSystem.ThrowIfNegative(value, nameof(value));
            _audioSystem.SetGroupGain(this, value);
            _gain = value;
        }
    }

    public void Pause()
    {
        _audioSystem.PauseGroup(this);
    }

    public void Resume()
    {
        _audioSystem.ResumeGroup(this);
    }

    public void Stop()
    {
        _audioSystem.StopGroup(this);
    }
}
