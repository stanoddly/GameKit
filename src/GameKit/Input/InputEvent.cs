namespace GameKit.Input;

public abstract class ConsumableInputEventArgs : EventArgs
{
    public bool Consumed { get; internal set; }

    public void Consume()
    {
        Consumed = true;
    }
}

public delegate void InputEventHandler<in TSender, in TEventArgs>(
    TSender sender,
    TEventArgs eventArgs)
    where TEventArgs : ConsumableInputEventArgs;
