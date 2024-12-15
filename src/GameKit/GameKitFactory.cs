using GameKit.Gpu;
using GameKit.Input;
using GameKit.Utilities;
using SDL;

namespace GameKit;

public record WindowConfiguration((int, int)? Size = null, string? Title = null);

public class GameKitFactory: IDisposable
{
    private static readonly (int, int) DefaultSize = (640, 480);

    private bool _initialized;
    
    private void EnsureSdlInitialized()
    {
        if (_initialized)
        {
            return;
        }

        if (SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO) == false)
        {
            throw new GameKitInitializationException($"SDL_Init failed: {SDL3.SDL_GetError()}");
        }

        _initialized = true;
    }

    public Window CreateWindow(WindowConfiguration config)
    {
        return CreateWindow(config.Size, config.Title);
    }
    
    private Window CreateWindow((int, int)? size = null, string? title=null)
    {
        EnsureSdlInitialized();

        string windowTitle;
        if (title == null)
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            windowTitle = process.ProcessName;
        }
        else
        {
            windowTitle = title;
        }
        
        (int width, int height) = size ?? DefaultSize;
        Pointer<SDL_Window> sdlWindow;
        unsafe
        {
             sdlWindow= SDL3.SDL_CreateWindow(windowTitle, width, height, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        }

        if (sdlWindow.IsNull())
        {
            throw new GameKitInitializationException($"SDL_CreateWindow failed: {SDL3.SDL_GetError()}");
        }
        
        return new Window(sdlWindow);
    }

    public GpuDevice CreateGpuDevice(Window window)
    {
        EnsureSdlInitialized();

        unsafe
        {
            Pointer<SDL_GPUDevice> device = SDL3.SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV, true, (byte*)null);
            if (device.IsNull())
            {
                throw new GameKitInitializationException($"SDL_CreateGPUDevice failed: {SDL3.SDL_GetError()}");
            }
        
            if (SDL3.SDL_ClaimWindowForGPUDevice(device, window.Pointer) == false)
            {
                throw new GameKitInitializationException($"GPUClaimWindow failed: {SDL3.SDL_GetError()}");
            }
            
            return new GpuDevice(device, window.Pointer);
        }
    }

    public InputService CreateInput()
    {
        EnsureSdlInitialized();
        
        return new InputService();
    }

    public EventService CreateEventService(InputService inputService, AppControl appControl)
    {
        EnsureSdlInitialized();

        return new EventService(inputService, appControl);
    }

    public TimeService CreateTimeService()
    {
        EnsureSdlInitialized();

        return new TimeService();
    }

    public void Dispose()
    {
        if (!_initialized)
        {
            return;
        }

        SDL3.SDL_Quit();
        _initialized = false;
    }
}