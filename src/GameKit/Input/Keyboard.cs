namespace GameKit.Input;

[System.Runtime.CompilerServices.InlineArray(16)]
internal struct InlineBuffer512
{
    private int _element0;    
}

public class Keyboard
{
    private InlineBuffer512 _bitArray;
    private const int IntBitSize = 32;
    private const int IntSizeMinusOne = IntBitSize - 1;
    private const int IntBitShift = 5;
    private const int One = 0x1;

    public bool IsPressed(Scancode scancode)
    {
        int index = (int)scancode;

        return (_bitArray[index >>> IntBitShift] & (One << (index & IntSizeMinusOne))) != 0;
    }
    
    internal bool Set(Scancode scancode)
    {
        int index = (int)scancode;

        ref var value = ref _bitArray[index >>> IntBitShift];
        int mask = One << (index & IntSizeMinusOne);
        bool wasUnset = (value & mask) == 0;
        value |= mask;
        return wasUnset;
    }
    
    internal void Unset(Scancode scancode)
    {
        int index = (int)scancode;
        
        ref var value = ref _bitArray[index >>> IntBitShift];
        int mask = One << (index & IntSizeMinusOne);
        value &= ~mask;
    }
    
    public bool Shift => IsPressed(Scancode.LeftShift) || IsPressed(Scancode.RightShift);
    public bool Alt => IsPressed(Scancode.LeftAlt) || IsPressed(Scancode.RightAlt);
    public bool Ctrl => IsPressed(Scancode.LeftCtrl) || IsPressed(Scancode.RightCtrl);
    public bool Gui => IsPressed(Scancode.LeftGui) || IsPressed(Scancode.RightGui);
    public bool Super => IsPressed(Scancode.LeftGui) || IsPressed(Scancode.RightGui);
}