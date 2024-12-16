using System.Collections.Immutable;
using System.Reflection;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Modules;
using GameKit.Shaders;
using SDL;

namespace GameKit;

public class GameKitApp
{
    private (int, int)? _windowSize;
    private string? _windowTitle;
    private bool? _debugMode;
    private List<IContentLoader<object>> _contentLoaders = new();
    private FileSystemBuilder _fileSystemBuilder = new();
    private GameModuleBuilder _gameModuleBuilder;

#if DEBUG
    private const bool DebugBuild = true;
#else
    private const bool DebugBuild = false;
#endif

    public GameKitApp WithSize((int, int) size)
    {
        _windowSize = size;
        return this;
    }
    
    public GameKitApp WithSize(int width, int height)
    {
        _windowSize = (width, height);
        return this;
    }

    public GameKitApp WithTitle(string title)
    {
        _windowTitle = title;
        return this;
    }

    public GameKitApp WithDebugMode()
    {
        _debugMode = true;
        return this;
    }

    public GameKitApp WithContentLoader<TContent>(IContentLoader<TContent> loaderRegistration) where TContent: class
    {
        _contentLoaders.Add(loaderRegistration);
        
        return this;
    }
    
    public GameKitApp AddContentFromDirectory(string directory)
    {
        _fileSystemBuilder.AddContentFromDirectory(directory);
        return this;
    }

    public GameKitApp AddContentFromProjectDirectory(string? subdirectory = null)
    {
        _fileSystemBuilder.AddContentFromProjectDirectory(subdirectory);
        
        return this;
    }

    public GameKitApp WithCachedFileSystem()
    {
        _fileSystemBuilder.WithCache();
        return this;
    }
    
    private GameModule Build()
    {
        GameKitFactory gameKitFactory = new GameKitFactory();
        _gameModuleBuilder.RegisterInstance(gameKitFactory);
        
        _gameModuleBuilder.RegisterFactory<Window, WindowConfiguration>(gameKitFactory.CreateWindow);
        _gameModuleBuilder.RegisterFactory<GpuDevice, Window>(gameKitFactory.CreateGpuDevice);
        _gameModuleBuilder.RegisterFactory<FrameContext>(gameKitFactory.CreateFrameContext);
        _gameModuleBuilder.RegisterFactory(gameKitFactory.CreateInput);

        _contentLoaders.Add(new ShaderPackLoader());

        _gameModuleBuilder.RegisterFactory(_fileSystemBuilder.Create);
        _gameModuleBuilder.Register<ContentManager>(Lifetime.Singleton);

        _gameModuleBuilder.Register<AppControl>(Lifetime.Singleton);
        _gameModuleBuilder.RegisterFactory<EventService, InputService, AppControl>(gameKitFactory.CreateEventService, Lifetime.Singleton);

        _gameModuleBuilder.Register<ShaderLoader>(Lifetime.Singleton);
        _gameModuleBuilder.Register<GraphicsPipelineBuilder>(Lifetime.Singleton);
        _gameModuleBuilder.Register<FrameContext>(Lifetime.Singleton);

        if (!_gameModuleBuilder.IsRegistered<WindowConfiguration>())
        {
            _gameModuleBuilder.Register<WindowConfiguration>(Lifetime.Singleton);
        }

        return _gameModuleBuilder.Build();
    }

    public int Run()
    {
        using GameModule gameModule = Build();

        FrameContext frameContext = gameModule.GetService<FrameContext>();
        EventService eventService = gameModule.GetService<EventService>();
        AppControl appControl = gameModule.GetService<AppControl>();
        
        while (true)
        {
            frameContext.StartFrame();
            eventService.Process();

            if (appControl.QuitRequested)
            {
                return 0;
            }
            
            gameModule.Update();
            gameModule.Draw();
        }
    }
}
