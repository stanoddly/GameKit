using System.Collections.Immutable;
using System.Reflection;
using GameKit.Content;
using GameKit.Input;
using SDL;

namespace GameKit;

public class GameKitAppBuilder
{

    private (int, int)? _windowSize;
    private string? _windowTitle;
    private bool? _debugMode;
    private List<IContentLoader<object>> _contentLoaders = new();
    private FileSystemBuilder _fileSystemBuilder = new();

#if DEBUG
    private const bool DebugBuild = true;
#else
    private const bool DebugBuild = false;
#endif

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
    
    public GameKitApp Build()
    {
        GameKitFactory gameKitFactory = new GameKitFactory();
        Window window = gameKitFactory.CreateWindow(_windowSize, _windowTitle);
        GpuDevice gpuDevice = gameKitFactory.CreateGpuDevice(window);

        _contentLoaders.Add(new ShaderPackLoader());

        VirtualFileSystem fileSystem = _fileSystemBuilder.Create();
        
        ContentManager contentManager = new ContentManager(fileSystem, _contentLoaders);

        AppControl appControl = new AppControl();
        InputService inputService = gameKitFactory.CreateInput();
        EventService eventService = gameKitFactory.CreateEventService(inputService, appControl);

        ImmutableArray<IDisposable> disposables = [contentManager, fileSystem, gpuDevice, window, gameKitFactory];

        return new GameKitApp
        {
            GpuDevice = gpuDevice,
            Window = window,
            ShaderLoader = new ShaderLoader(gpuDevice),
            GraphicsPipelineBuilder = new GraphicsPipelineBuilder(gpuDevice, window),
            FileSystem = fileSystem,
            ContentManager = contentManager,
            Input = inputService,
            Events = eventService,
            AppControl = appControl,
            Disposables = disposables
        };
    }
}
