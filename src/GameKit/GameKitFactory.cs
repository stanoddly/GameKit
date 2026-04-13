using System.Runtime.InteropServices;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Shaders;
using GameKit.Text;
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

        if (_config.DebugLogging)
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

    public IWindow CreateWindow(IGpuDevice gpuDevice, AppConfig config)
    {
        return CreateWindowInternal((GpuDevice)gpuDevice, config.Size, config.Title, config.Fullscreen);
    }

    private Window CreateWindowInternal(GpuDevice gpuDevice, Size<uint>? size = null, string? title = null, bool fullscreen = false)
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
        Pointer<SDL_Window> sdlWindow;
        unsafe
        {
             sdlWindow= SDL3.SDL_CreateWindow(windowTitle, (int)width, (int)height, fullscreen ? SDL_WindowFlags.SDL_WINDOW_FULLSCREEN : 0);
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

        return new Window(sdlWindow, gpuDevice.SdlGpuDevice, sdlWindowId);
    }

    public IGpuDevice CreateGpuDevice()
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

    public IKeyboardService CreateKeyboardService(AppControl appControl)
    {
        EnsureSdlInitialized();

        return new KeyboardService(appControl);
    }

    public IGamepadService CreateGamepadService()
    {
        EnsureSdlInitialized();

        GamepadService gamepadService = new();
        gamepadService.SetupGamepads();

        return gamepadService;
    }

    public IMouseService CreateMouseService()
    {
        EnsureSdlInitialized();

        return new MouseService();
    }

    public EventService CreateEventService(IKeyboardService keyboardService, IGamepadService gamepadService, IMouseService mouseService, IWindow window, AppControl appControl)
    {
        EnsureSdlInitialized();

        return new EventService((KeyboardService)keyboardService, (GamepadService)gamepadService, (MouseService)mouseService, (Window)window, appControl);
    }

    public GameKitFrameContext CreateFrameContext()
    {
        return new GameKitFrameContext();
    }

    public IContentLoader<Image> CreateImageLoader(VirtualFileSystem fileSystem)
    {
        return new SdlImageLoader(fileSystem);
    }

    public IFontSystem CreateFontSystem(GpuMemorySystem gpuMemorySystem, VirtualFileSystem fileSystem)
    {
        return FontSystem.Create(gpuMemorySystem, fileSystem);
    }

    public IContentLoader<Shader> CreateShaderLoader(IGpuDevice gpuDevice, ShaderMetadataLoader shaderMetadataLoader, VirtualFileSystem fileSystem)
    {
        return new ShaderLoader((GpuDevice)gpuDevice, shaderMetadataLoader, fileSystem);
    }

    public GraphicsPipelineBuilder CreateGraphicsPipelineBuilder(IGpuDevice gpuDevice, IWindow window, IContentLoader<Shader> shaderLoader)
    {
        return new GraphicsPipelineBuilder((GpuDevice)gpuDevice, (Window)window, (ShaderLoader)shaderLoader);
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
