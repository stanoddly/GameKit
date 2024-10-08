using SDL;

namespace GameKit;

public class GameKitException : Exception
{
    public GameKitException()
    {
    }

    public GameKitException(string? message)
        : base(message)
    {
    }

    public GameKitException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}

public static class SdlError
{
    public static void Clear()
    {
        SDL3.SDL_ClearError();
    }
    
    public static void ThrowOnFalse(bool value)
    {
        if (!value) throw new GameKitException(SDL3.SDL_GetError());
    }

    public static void ThrowOnNull<T>(Pointer<T> pointer) where T : unmanaged
    {
        if (pointer.IsNull()) throw new GameKitException(SDL3.SDL_GetError());
    }
    
    public static unsafe void ThrowOnNull<T>(T* pointer) where T : unmanaged
    {
        if (pointer == null) throw new GameKitException(SDL3.SDL_GetError());
    }
    
    public static void ThrowOnError()
    {
        string? error = SDL3.SDL_GetError();
        if (!string.IsNullOrEmpty(error))
        {
            throw new GameKitException(error);
        }
    }
}