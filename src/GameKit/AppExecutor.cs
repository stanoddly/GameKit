using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL;

namespace GameKit;

internal static class GameKitCallbackAppExecutor
{
    private static GameKitApp _app = null!;
    
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "AppInit")]
    private static unsafe SDL_AppResult AppInit(IntPtr* appState, int argc, byte** argv)
    {
        // this is dummy, the setup is done before the call
        // TODO: to be verified
        // https://discourse.libsdl.org/t/is-it-safe-to-call-sdl-init-before-sdl-enterappmaincallbacks/55434
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "AppIterate")]
    private static unsafe SDL_AppResult AppIterate(IntPtr appState)
    {
        _app.Events.Process();

        if (_app.AppControl.QuitRequested)
        {
            return SDL_AppResult.SDL_APP_SUCCESS;
        }
        
        _app.DoUpdate();
        
        if (_app.AppControl.QuitRequested)
        {
            return SDL_AppResult.SDL_APP_SUCCESS;
        }

        _app.DoDraw();
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "AppEvent")]
    private static unsafe SDL_AppResult AppEvent(IntPtr appState, SDL_Event* e)
    {
        _app.Events.EnqueueEvent(e);

        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "AppQuit")]
    private static unsafe void AppQuit(IntPtr appState, SDL_AppResult result)
    {
        _app.CleanUp();
    }

    internal static int Execute(GameKitApp gameKitApp)
    {
        if (_app != null)
        {
            throw new Exception();
        }

        _app = gameKitApp;
        unsafe
        {
            return SDL3.SDL_EnterAppMainCallbacks(0, null, &AppInit, &AppIterate, &AppEvent, &AppQuit);
        }
    }
}