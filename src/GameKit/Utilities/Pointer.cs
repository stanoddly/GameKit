namespace GameKit.Utilities;

public readonly struct Pointer<TValue> where TValue : unmanaged
{
    public static readonly Pointer<TValue> Null = default;

    private readonly unsafe TValue* _rawPointer;
    
    public unsafe Pointer(TValue* rawPointer) => _rawPointer = rawPointer;
    
    public static unsafe implicit operator TValue*(Pointer<TValue> pointer)
    {
        return pointer._rawPointer;
    }
    
    public static unsafe implicit operator Pointer<TValue>(TValue* rawPointer)
    {
        return new Pointer<TValue>(rawPointer);
    }
    
    public ref TValue Value
    {
        get
        {
            unsafe
            {
                if (_rawPointer == null) throw new InvalidOperationException("Pointer is null.");
                return ref *_rawPointer;
            }
        }
    }

    public bool IsNull()
    {
        unsafe { return _rawPointer == null;}
    }

    public void ThrowIfNull()
    {
        unsafe
        {
            if (_rawPointer == null) throw new InvalidOperationException("Pointer is null.");
        }
    }
    
    public void ThrowIfNull(string message)
    {
        unsafe
        {
            if (_rawPointer == null) throw new InvalidOperationException(message);
        }
    }
}
