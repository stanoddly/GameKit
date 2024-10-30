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

    public bool IsPressed(ScanCode scanCode)
    {
        int index = (int)scanCode;

        return (_bitArray[index >>> IntBitShift] & (One << (index & IntSizeMinusOne))) != 0;
    }
    
    internal bool Set(ScanCode scanCode)
    {
        int index = (int)scanCode;

        ref var value = ref _bitArray[index >>> IntBitShift];
        int mask = One << (index & IntSizeMinusOne);
        bool wasUnset = (value & mask) == 0;
        value |= mask;
        return wasUnset;
    }
    
    internal void Unset(ScanCode scanCode)
    {
        int index = (int)scanCode;
        
        ref var value = ref _bitArray[index >>> IntBitShift];
        int mask = One << (index & IntSizeMinusOne);
        value &= ~mask;
    }
    
    public bool Shift => IsPressed(ScanCode.LeftShift) || IsPressed(ScanCode.RightShift);
    public bool Alt => IsPressed(ScanCode.LeftAlt) || IsPressed(ScanCode.RightAlt);
    public bool Ctrl => IsPressed(ScanCode.LeftCtrl) || IsPressed(ScanCode.RightCtrl);
    public bool Gui => IsPressed(ScanCode.LeftGui) || IsPressed(ScanCode.RightGui);
    public bool Super => IsPressed(ScanCode.LeftGui) || IsPressed(ScanCode.RightGui);
}