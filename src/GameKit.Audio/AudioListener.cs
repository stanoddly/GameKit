using System.Numerics;

namespace GameKit.Audio;

public sealed class AudioListener
{
    private readonly AudioSystem _audioSystem;
    private Vector3 _position;

    internal AudioListener(AudioSystem audioSystem)
    {
        _audioSystem = audioSystem;
    }

    public Vector3 Position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;
            _audioSystem.UpdateSourcePositions();
        }
    }

    public float Gain
    {
        get
        {
            return _audioSystem.MasterGain;
        }
        set
        {
            _audioSystem.MasterGain = value;
        }
    }
}
