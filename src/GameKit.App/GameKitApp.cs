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
    private readonly FileSystemBuilder _fileSystemBuilder = new();
    private readonly GameModuleBuilder _gameModuleBuilder = new();

#if DEBUG
    private const bool DebugBuild = true;
#else
    private const bool DebugBuild = false;
#endif

    public GameKitApp AddContentFromDirectory(string directory)
    {
        _fileSystemBuilder.AddContentFromDirectory(directory);
        
        return this;
    }
    
    public GameKitApp AddContentFromProjectDirectory(string directory)
    {
        _fileSystemBuilder.AddContentFromProjectDirectory(directory);
        return this;
    }
    
    public GameKitApp AddFileSystemCache()
    {
        _fileSystemBuilder.WithCache();
        return this;
    }

    public GameKitApp AddScoped<TSourceType, TTargetType>()
        where TSourceType: TTargetType
        where TTargetType : notnull
    {
        _gameModuleBuilder.Register<TSourceType, TTargetType>();
        
        return this;
    }

    public GameKitApp AddScoped<TType>() where TType : notnull
    {
        _gameModuleBuilder.Register<TType>();
        
        return this;
    }
    
    public GameKitApp Add<TType>(TType instance) where TType : class
    {
        _gameModuleBuilder.RegisterInstance(instance);
        
        return this;
    }

    private GameModule Build()
    {
        GameKitFactory gameKitFactory = new GameKitFactory();
        _gameModuleBuilder.RegisterInstance(gameKitFactory);
        
        _gameModuleBuilder.RegisterFactory<Window, WindowConfiguration>(gameKitFactory.CreateWindow);
        _gameModuleBuilder.RegisterFactory<GpuDevice, Window>(gameKitFactory.CreateGpuDevice);
        _gameModuleBuilder.RegisterFactory(gameKitFactory.CreateFrameContext);
        _gameModuleBuilder.RegisterFactory(gameKitFactory.CreateInput);
        _gameModuleBuilder.RegisterFactory<EventService, InputService, AppControl>(gameKitFactory.CreateEventService, Lifetime.Singleton);

        _gameModuleBuilder.Register<AppControl>(Lifetime.Singleton);
        _gameModuleBuilder.Register<ShaderPackLoader, IContentLoader<ShaderPack>>(Lifetime.Singleton);

        _gameModuleBuilder.Register<ShaderLoader, IContentLoader<Shader>>(Lifetime.Singleton);
        _gameModuleBuilder.Register<GraphicsPipelineBuilder>(Lifetime.Singleton);

        if (!_gameModuleBuilder.IsRegistered<WindowConfiguration>())
        {
            _gameModuleBuilder.Register<WindowConfiguration>(Lifetime.Singleton);
        }

        if (!_gameModuleBuilder.IsRegistered<VirtualFileSystem>())
        {
            _gameModuleBuilder.RegisterFactory(_fileSystemBuilder.Create);
            //_gameModuleBuilder.RegisterInstance<VirtualFileSystem>(DictFileSystem.Empty);
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
