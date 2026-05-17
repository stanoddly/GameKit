using System.Runtime.InteropServices;
using SDL;

namespace GameKit.Input;

public class ClipboardService : IClipboardService
{
    public bool HasText
    {
        get
        {
            unsafe
            {
                return SDL3.SDL_HasClipboardText();
            }
        }
    }

    public string? GetText()
    {
        unsafe
        {
            byte* ptr = SDL3.Unsafe_SDL_GetClipboardText();
            return Marshal.PtrToStringUTF8((IntPtr)ptr);
        }
    }

    public void SetText(string text)
    {
        SDL3.SDL_SetClipboardText(text);
    }
}
