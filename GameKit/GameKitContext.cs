using SDL;

namespace GameKit;

public class GameKitContext: IDisposable
{
    public Window Window { get; init; }
    public GpuDevice GpuDevice { get; init; }

    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_ReleaseWindowFromGPUDevice(GpuDevice.SdlGpuDevice, Window.Pointer);
        }
        GpuDevice.Dispose();
        Window.Dispose();
    }

    public static GameKitContextBuilder Builder()
    {
        return new GameKitContextBuilder();
    }

    public bool ConsumeEventsAndFalseOnQuit()
    {
        unsafe
        {
            SDL_Event evt;
            while (SDL3.SDL_PollEvent(&evt) == SDL_bool.SDL_TRUE)
            {
                if (evt.Type == SDL_EventType.SDL_EVENT_QUIT)
                {
                    return false;
                }
            }
        }

        return true;
    }
}