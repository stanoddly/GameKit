using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL;

namespace GameKit;

public interface IDrawable
{
    public void Draw();
}

public interface IUpdateable
{
    public void Update();
}



public class SdlLifecycleWrapper
{
    private readonly GameKitApp _app;

    public SdlLifecycleWrapper(GameKitApp app)
    {
        _app = app;
        SDL3.SDL_EnterAppMainCallbacks()
    }

    public unsafe delegate SDL_AppResult AppInit(IntPtr* appState, int argc, byte** argv);
    public unsafe delegate SDL_AppResult AppIterate(IntPtr appState);
    public unsafe delegate SDL_AppResult AppEvent(IntPtr appState, SDL_Event* e);
    public unsafe delegate void AppQuit(IntPtr appState, SDL_AppResult result);

    /*
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "AppIterate")]
    internal static unsafe SDL_AppResult Iterate(void* appState)
    {

        return SDL_AppResult.SDL_APP_FAILURE;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "AppInit")]
    internal static unsafe SDL_AppResult AppInit(void** appState, int argc, byte** argv)
    {
        return SDL_AppResult.SDL_APP_FAILURE;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "AppIterate")]
    internal static unsafe SDL_AppResult AppIterate(void* appState)
    {

        return SDL_AppResult.SDL_APP_FAILURE;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "AppEvent")]
    internal static unsafe SDL_AppResult AppEvent(void* appState, SDL_Event* e)
    {
        return SDL_AppResult.SDL_APP_FAILURE;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "AppEvent")]
    internal static unsafe void SDL_AppQuit(void *appState, SDL_AppResult result)
    {
    }
    */
}