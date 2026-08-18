namespace Pixely.Audio;

public abstract class AudioClip : IAudioClip
{
    internal AudioClip()
    {
    }

    internal abstract AudioSystem AudioSystem { get; }

    internal abstract void AttachTo(AudioSource source);
    internal abstract void DetachFrom(AudioSource source);

    public abstract void Dispose();
}
