using System.Runtime.InteropServices;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Utilities;
using SDL;

namespace GameKit;

public class GameKitFactory: IDisposable
{
    private static readonly Size<uint> DefaultSize = (640, 480);

    private readonly GameKitConfig _config;
    private bool _initialized;

    public GameKitFactory(GameKitConfig config)
    {
        _config = config;
    }

    private void EnsureSdlInitialized()
    {
        if (_initialized)
        {
            return;
        }

        //SDL3.SDL_SetHint(SDL3.SDL_HINT_EVENT_LOGGING, "2");
        //SDL3.SDL_SetHint(SDL3.SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS, "1");

        if (_config.EnableSdlLogging)
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

    internal Window CreateWindow(GpuDevice gpuDevice, GameKitFrameContext frameContext, AppConfig config)
    {
        return CreateWindow(gpuDevice, frameContext, config.Size, config.Title, config.Fullscreen, config.Resizable, config.Transparent, config.Borderless, config.AlwaysOnTop);
    }

    private Window CreateWindow(GpuDevice gpuDevice, GameKitFrameContext frameContext, Size<uint>? size = null, string? title = null, bool fullscreen = false, bool resizable = false, bool transparent = false, bool borderless = false, bool alwaysOnTop = false)
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

        (uint width, uint height) = fullscreen ? (0, 0) : size ?? DefaultSize;
        SDL_WindowFlags windowFlags = 0;
        if (fullscreen)
        {
            windowFlags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
        }

        if (resizable)
        {
            windowFlags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        if (transparent)
        {
            windowFlags |= SDL_WindowFlags.SDL_WINDOW_TRANSPARENT;
        }

        if (borderless)
        {
            windowFlags |= SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
        }

        if (alwaysOnTop)
        {
            windowFlags |= SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP;
        }

        Pointer<SDL_Window> sdlWindow;
        unsafe
        {
             sdlWindow = SDL3.SDL_CreateWindow(windowTitle, (int)width, (int)height, windowFlags);
        }

        if (sdlWindow.IsNull)
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

        return new Window(sdlWindow, gpuDevice.SdlGpuDevice, sdlWindowId, frameContext);
    }

    internal GpuDevice CreateGpuDevice()
    {
        EnsureSdlInitialized();

        unsafe
        {
            VkPhysicalDeviceShaderDrawParametersFeatures shaderDrawParamsFeatures = default;
            shaderDrawParamsFeatures.sType = VkPhysicalDeviceShaderDrawParametersFeatures.StructureType;
            shaderDrawParamsFeatures.shaderDrawParameters = 1;

            SDL_GPUVulkanOptions vulkanOptions = default;
            vulkanOptions.vulkan_api_version = (1 << 22) | (3 << 12) | 0;
            vulkanOptions.feature_list = (IntPtr)(&shaderDrawParamsFeatures);

            SDL_PropertiesID props = SDL3.SDL_CreateProperties();
            SDL_GPUVulkanOptions* vulkanOptionsPointer = &vulkanOptions;
            SDL3.SDL_SetBooleanProperty(props, SDL3.SDL_PROP_GPU_DEVICE_CREATE_DEBUGMODE_BOOLEAN, _config.EnableGpuValidation);
            SDL3.SDL_SetBooleanProperty(props, SDL3.SDL_PROP_GPU_DEVICE_CREATE_SHADERS_SPIRV_BOOLEAN, true);
            SDL3.SDL_SetPointerProperty(props, SDL3.SDL_PROP_GPU_DEVICE_CREATE_VULKAN_OPTIONS_POINTER, (IntPtr)vulkanOptionsPointer);

            Pointer<SDL_GPUDevice> device = SDL3.SDL_CreateGPUDeviceWithProperties(props);
            SDL3.SDL_DestroyProperties(props);

            if (device.IsNull)
            {
                throw new GameKitInitializationException($"SDL_CreateGPUDevice failed: {SDL3.SDL_GetError()}");
            }
            
            return new GpuDevice(device);
        }
    }

    internal KeyboardService CreateKeyboardService(AppControl appControl)
    {
        EnsureSdlInitialized();

        return new KeyboardService(appControl);
    }
    
    internal GamepadService CreateGamepadService()
    {
        EnsureSdlInitialized();
        
        GamepadService gamepadService = new();
        gamepadService.SetupGamepads();
        
        return gamepadService;
    }

    internal MouseService CreateMouseService()
    {
        EnsureSdlInitialized();

        return new MouseService();
    }

    internal TextInputService CreateTextInputService(Window window)
    {
        EnsureSdlInitialized();

        return new TextInputService(window);
    }

    internal EventService CreateEventService(KeyboardService keyboardService, GamepadService gamepadService, MouseService mouseService, TextInputService textInputService, Window window, AppControl appControl)
    {
        EnsureSdlInitialized();

        return new EventService(keyboardService, gamepadService, mouseService, textInputService, window, appControl);
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

[StructLayout(LayoutKind.Sequential)]
internal struct VkPhysicalDeviceShaderDrawParametersFeatures
{
    public const uint StructureType = 1000063000;

    public uint sType;
    public IntPtr pNext;
    public uint shaderDrawParameters;
}
