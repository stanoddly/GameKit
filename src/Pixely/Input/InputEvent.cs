namespace Pixely.Input;

public abstract class ConsumableInputEventArgs : EventArgs
{
    public bool Consumed { get; internal set; }

    public void Consume()
    {
        Consumed = true;
    }
}

public delegate void InputEventHandler<in TEventArgs>(TEventArgs eventArgs)
    where TEventArgs : ConsumableInputEventArgs;
