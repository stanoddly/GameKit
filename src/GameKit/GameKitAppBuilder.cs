using System.Reflection;
using GameKit.Content;
using SDL;

namespace GameKit;

public class GameKitAppBuilder
{
    private static readonly string DefaultTitle = "GameKit App";
    private static readonly (int, int) DefaultSize = (640, 480);
    private (int, int)? _windowSize;
    private string? _windowTitle;
    private bool? _debugMode;
    private List<IContentLoader<object>> _contentLoaders;
    private FileSystemBuilder _fileSystemBuilder;

#if DEBUG
    private const bool DebugBuild = true;
#else
    private const bool DebugBuild = false;
#endif
    
    public GameKitAppBuilder()
    {
        _contentLoaders = new List<IContentLoader<object>>();
        _fileSystemBuilder = new FileSystemBuilder();
    }
    

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

    public GameKitAppBuilder WithContentLoader<TContent>(IContentLoader<TContent> loaderRegistration) where TContent: class
    {
        _contentLoaders.Add(loaderRegistration);
        
        return this;
    }
    
    public GameKitAppBuilder AddContentFromDirectory(string directory)
    {
        _fileSystemBuilder.AddContentFromDirectory(directory);
        return this;
    }

    public GameKitAppBuilder AddContentFromProjectDirectory(string? subdirectory = null)
    {
        _fileSystemBuilder.AddContentFromProjectDirectory(subdirectory);
        
        return this;
    }

    public GameKitAppBuilder WithCachedFileSystem()
    {
        _fileSystemBuilder.WithCache();
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

        _contentLoaders.Add(new ShaderPackLoader());

        VirtualFileSystem fileSystem = _fileSystemBuilder.Create();
        
        ContentManager contentManager = new ContentManager(fileSystem, _contentLoaders);

        GpuDevice gpuDevice = new GpuDevice(device, sdlWindow);
        Window window = new Window(sdlWindow);
        return new GameKitApp
        {
            GpuDevice = gpuDevice,
            Window = window,
            ShaderLoader = new ShaderLoader(gpuDevice),
            GpuMemoryUploader = new GpuMemoryUploader(gpuDevice),
            GraphicsPipelineBuilder = new GraphicsPipelineBuilder(gpuDevice, window),
            FileSystem = fileSystem,
            ContentManager = contentManager
        };
    }
}
