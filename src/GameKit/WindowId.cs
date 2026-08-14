namespace GameKit;

public readonly record struct WindowId
{
    internal ulong Value { get; }

    internal WindowId(ulong value)
    {
        Value = value;
    }
}
