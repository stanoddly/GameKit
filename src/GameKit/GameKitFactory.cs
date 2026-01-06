using GameKit.Gpu;
using GameKit.Input;
using GameKit.Utilities;
using SDL;

namespace GameKit;

public class GameKitFactory: IDisposable
{
    private static readonly Size<uint> DefaultSize = (640, 480);

    private bool _initialized;
    private GameKitConfig? _config;

    private void EnsureSdlInitialized(GameKitConfig config)
    {
        if (_initialized)
        {
            return;
        }

        _config = config;

        //SDL3.SDL_SetHint(SDL3.SDL_HINT_EVENT_LOGGING, "2");
        //SDL3.SDL_SetHint(SDL3.SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS, "1");

        if (config.DebugLogging)
        {
            SDL3.SDL_SetHint(SDL3.SDL_HINT_LOGGING, "*=debug");
        }

        SDL_InitFlags initFlags = SDL_InitFlags.SDL_INIT_EVENTS | SDL_InitFlags.SDL_INIT_VIDEO |
                                  SDL_InitFlags.SDL_INIT_JOYSTICK | SDL_InitFlags.SDL_INIT_GAMEPAD;
        if (SDL3.SDL_Init(initFlags) == false)
        {
            throw new GameKitInitializationException($"SDL_Init failed: {SDL3.SDL_GetError()}");
        }

        _initialized = true;
    }

    internal Window CreateWindow(GpuDevice gpuDevice, AppConfig config)
    {
        return CreateWindow(gpuDevice, config.Size, config.Title, config.Fullscreen);
    }
    
    private Window CreateWindow(GpuDevice gpuDevice, Size<uint>? size = null, string? title=null, bool fullscreen = false)
    {
        EnsureSdlInitialized(_config ?? new GameKitConfig());

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
        
        (uint width, uint height) = fullscreen ? (0, 0) : size ?? DefaultSize;
        Pointer<SDL_Window> sdlWindow;
        unsafe
        {
             sdlWindow= SDL3.SDL_CreateWindow(windowTitle, (int)width, (int)height, fullscreen ? SDL_WindowFlags.SDL_WINDOW_FULLSCREEN : 0);
        }

        if (sdlWindow.IsNull())
        {
            throw new GameKitInitializationException($"SDL_CreateWindow failed: {SDL3.SDL_GetError()}");
        }

        unsafe
        {
            if (SDL3.SDL_ClaimWindowForGPUDevice(gpuDevice.SdlGpuDevice, sdlWindow) == false)
            {
                throw new GameKitInitializationException($"GPUClaimWindow failed: {SDL3.SDL_GetError()}");
            }
        }

        uint sdlWindowId;
        unsafe
        {
            sdlWindowId = (uint)SDL3.SDL_GetWindowID(sdlWindow);
            
            if (sdlWindowId == 0)
            {
                throw new GameKitInitializationException($"GPUClaimWindow failed: {SDL3.SDL_GetError()}");
            }
        }
        
        return new Window(sdlWindow, gpuDevice.SdlGpuDevice, sdlWindowId);
    }

    internal GpuDevice CreateGpuDevice(GameKitConfig config)
    {
        EnsureSdlInitialized(config);

        unsafe
        {
            SDL_GPUVulkanOptions vulkanOptions = default;
            vulkanOptions.vulkan_api_version = (1 << 22) | (3 << 12) | 0;

            SDL_PropertiesID props = SDL3.SDL_CreateProperties();
            SDL_GPUVulkanOptions* vulkanOptionsPointer = &vulkanOptions;
            SDL3.SDL_SetBooleanProperty(props, SDL3.SDL_PROP_GPU_DEVICE_CREATE_SHADERS_SPIRV_BOOLEAN, true);
            SDL3.SDL_SetPointerProperty(props, SDL3.SDL_PROP_GPU_DEVICE_CREATE_VULKAN_OPTIONS_POINTER, (IntPtr)vulkanOptionsPointer);

            Pointer<SDL_GPUDevice> device = SDL3.SDL_CreateGPUDeviceWithProperties(props);
            SDL3.SDL_DestroyProperties(props);

            if (device.IsNull())
            {
                throw new GameKitInitializationException($"SDL_CreateGPUDevice failed: {SDL3.SDL_GetError()}");
            }
            
            return new GpuDevice(device);
        }
    }

    internal KeyboardService CreateKeyboardService(AppControl appControl)
    {
        EnsureSdlInitialized(_config ?? new GameKitConfig());
        
        KeyboardService keyboardService = new(appControl);

        return keyboardService;
    }
    
    internal GamepadService CreateGamepadService()
    {
        EnsureSdlInitialized(_config ?? new GameKitConfig());
        
        GamepadService gamepadService = new();
        gamepadService.SetupGamepads();
        
        return gamepadService;
    }

    internal MouseService CreateMouseService()
    {
        EnsureSdlInitialized(_config ?? new GameKitConfig());
        
        return new MouseService();
    }

    public EventService CreateEventService(KeyboardService keyboardService, GamepadService gamepadService, MouseService mouseService, AppControl appControl)
    {
        EnsureSdlInitialized(_config ?? new GameKitConfig());
        
        return new EventService(keyboardService, gamepadService, mouseService, appControl);
    }

    public GameKitFrameContext CreateFrameContext()
    {
        return new GameKitFrameContext();
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