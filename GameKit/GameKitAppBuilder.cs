using System.IO.Abstractions;
using System.Reflection;
using SDL;

namespace GameKit;

public struct GameKitAppBuilder
{
#if DEBUG
    private const bool DebugBuild = true;
#else
    private const bool DebugBuild = false;
#endif

    private static readonly string DefaultTitle = "GameKit App";
    private static readonly (int, int) DefaultSize = (640, 480);
    private (int, int)? _windowSize;
    private string? _windowTitle;
    private bool? _debugMode;
    private List<IContentLoaderRegistration>? _loaderRegistrations;
    

    public GameKitApp Build()
    {
        return UnsafeBuild();
    }

    public GameKitAppBuilder WithSize((int, int) size)
    {
        _windowSize = size;
        return this;
    }
    
    public GameKitAppBuilder WithSize(int width, int height)
    {
        _windowSize = (width, height);
        return this;
    }

    public GameKitAppBuilder WithTitle(string title)
    {
        _windowTitle = title;
        return this;
    }

    public GameKitAppBuilder WithDebugMode()
    {
        _debugMode = true;
        return this;
    }

    public GameKitAppBuilder WithRootDirectoryFromExecutable(string? subdirectory = null)
    {
        string? executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // no need to change anything
        if (executableDirectory == null && subdirectory == null)
        {
            return this;
        }
        
        if (subdirectory == null)
        {
            Directory.SetCurrentDirectory(executableDirectory!);
            return this;
        }

        // we are at root
        if (executableDirectory == null)
        {
            Directory.SetCurrentDirectory(subdirectory!);
            return this;
        }
        
        Directory.SetCurrentDirectory(Path.Combine(executableDirectory, "Content"));
        return this;
    }

    public GameKitAppBuilder WithContentLoader(IContentLoaderRegistration loaderRegistration)
    {
        _loaderRegistrations ??= new List<IContentLoaderRegistration>();
        _loaderRegistrations.Add(loaderRegistration);
        
        return this;
    }

    private unsafe GameKitApp UnsafeBuild()
    {
        if (SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO) == SDL_bool.SDL_FALSE)
        {
            throw new GameKitInitializationException($"SDL_Init failed: {SDL3.SDL_GetError()}");
        }
        
        Pointer<SDL_GPUDevice> device = SDL3.SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV, SDL_bool.SDL_TRUE, (byte*)null);
        if (device.IsNull())
        {
            throw new GameKitInitializationException($"SDL_CreateGPUDevice failed: {SDL3.SDL_GetError()}");
        }

        string windowTitle;
        if (_windowTitle == null)
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            windowTitle = process.ProcessName;
        }
        else
        {
            windowTitle = _windowTitle;
        }
        
        (int width, int height) = _windowSize ?? DefaultSize;
        Pointer<SDL_Window> sdlWindow = SDL3.SDL_CreateWindow(windowTitle, width, height, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        if (sdlWindow.IsNull())
        {
            throw new GameKitInitializationException($"SDL_CreateWindow failed: {SDL3.SDL_GetError()}");
        }

        if (SDL3.SDL_ClaimWindowForGPUDevice(device, sdlWindow) == SDL_bool.SDL_FALSE)
        {
            throw new GameKitInitializationException($"GPUClaimWindow failed: {SDL3.SDL_GetError()}");
        }

        ContentManager contentManager = new ContentManager(new VirtualFileSystem());

        _loaderRegistrations ??= new List<IContentLoaderRegistration>();
        _loaderRegistrations.Add(new ShaderPackLoader());
        
        foreach (IContentLoaderRegistration contentLoaderRegistration in _loaderRegistrations)
        {
            contentLoaderRegistration.Register(contentManager);
        }

        GpuDevice gpuDevice = new GpuDevice(device, sdlWindow);
        Window window = new Window(sdlWindow);
        return new GameKitApp
        {
            GpuDevice = gpuDevice,
            Window = window,
            ShaderLoader = new ShaderLoader(gpuDevice),
            GpuMemoryUploader = new GpuMemoryUploader(gpuDevice),
            GraphicsPipelineBuilder = new GraphicsPipelineBuilder(gpuDevice, window),
            ContentManager = contentManager
        };
    }
}
