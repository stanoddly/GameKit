using System.Collections.Immutable;
using System.Reflection;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Modules;
using GameKit.Shaders;
using SDL;

namespace GameKit;

public class GameKitAppBuilder
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
    
    private GameModule Build()
    {
        GameKitFactory gameKitFactory = new GameKitFactory();
        _gameModuleBuilder.RegisterInstance(gameKitFactory);
        
        _gameModuleBuilder.RegisterFactory<Window, WindowConfiguration>(gameKitFactory.CreateWindow);
        _gameModuleBuilder.RegisterFactory<GpuDevice, Window>(gameKitFactory.CreateGpuDevice);
        _gameModuleBuilder.RegisterFactory<TimeService>(gameKitFactory.CreateTimeService);
        _gameModuleBuilder.RegisterFactory(gameKitFactory.CreateInput);

        _contentLoaders.Add(new ShaderPackLoader());

        _gameModuleBuilder.RegisterFactory(_fileSystemBuilder.Create);
        _gameModuleBuilder.Register<ContentManager>(Lifetime.Singleton);

        _gameModuleBuilder.Register<AppControl>(Lifetime.Singleton);
        _gameModuleBuilder.RegisterFactory<EventService, InputService, AppControl>(gameKitFactory.CreateEventService, Lifetime.Singleton);

        _gameModuleBuilder.Register<ShaderLoader>(Lifetime.Singleton);
        _gameModuleBuilder.Register<GraphicsPipelineBuilder>(Lifetime.Singleton);
        _gameModuleBuilder.Register<FrameContext>(Lifetime.Singleton);

        return _gameModuleBuilder.Build();
    }

    public int Run()
    {
        using GameModule gameModule = Build();

        while (true)
        {
            gameModule.BeginFrame((float)gameTime.ElapsedGameTime.TotalSeconds);
            gameModule.Update();
            
            gameModule.Draw();
        }
    }
}
