using GameKit.BackgroundJobs;
using GameKit.Common;
using GameKit.Ioc;
using GameKit.Content;
using GameKit.Encs;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Shaders;
using GameKit.Text;
using Microsoft.Extensions.DependencyInjection;

namespace GameKit.App;

public class GameKitAppBuilder
{
    private readonly GameModuleBuilder _moduleBuilder = new();
    private readonly FileSystemBuilder _fileSystemBuilder = new();
    private readonly List<IStartable> _startables = new();
    private readonly List<IUpdatable> _updatables = new();
    private readonly List<IDisposable> _disposables = new();
    private readonly List<Action<IServiceProvider, BackgroundJobWorkerPool>> _processorRegistrations = new();

    public GameKitAppBuilder()
    {
        EventBus eventBus = new();
        _moduleBuilder.OnActivated(obj =>
        {
            if (obj is IStartable startable)
            {
                _startables.Add(startable);
            }
            if (obj is IUpdatable updatable)
            {
                _updatables.Add(updatable);
            }
            if (obj is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }

            eventBus.Subscribe(obj);
        });
        
        _moduleBuilder.RegisterInstance(eventBus);
    }

    public GameKitAppBuilder OnActivated(Action<object> callback)
    {
        _moduleBuilder.OnActivated(callback);
        return this;
    }

    public GameModuleRegistrar<TImplementation> RegisterType<TImplementation>() where TImplementation : class
    {
        return _moduleBuilder.RegisterType<TImplementation>();
    }

    public GameModuleRegistrar<TImplementation> RegisterInstance<TImplementation>(TImplementation instance) where TImplementation : class
    {
        return _moduleBuilder.RegisterInstance(instance);
    }
    
    public GameModuleRegistrar<TService> RegisterFunc<TService>(Delegate factory)
        where TService : class
    {
        return _moduleBuilder.RegisterFunc<TService>(factory);
    }

    public GameKitAppBuilder AddContentFromDirectory(string directory)
    {
        _fileSystemBuilder.AddContentFromDirectory(directory);
        return this;
    }

    public GameKitAppBuilder AddFileSystem(VirtualFileSystem fileSystem)
    {
        _fileSystemBuilder.AddSourceFileSystem(fileSystem);
        return this;
    }

    public GameKitAppBuilder AddContentFromProjectDirectory(string directory)
    {
        _fileSystemBuilder.AddContentFromProjectDirectory(directory);
        return this;
    }

    public GameKitAppBuilder AddContentFromZipPattern(string pattern)
    {
        string[] filenames = Directory.GetFiles(Directory.GetCurrentDirectory(), pattern);
        foreach (string filename in filenames)
        {
            _fileSystemBuilder.AddContentFromZip(filename);
        }
        return this;
    }

    public GameKitAppBuilder AddFileSystemCache()
    {
        _fileSystemBuilder.WithCache();
        return this;
    }

    public IGameKitApp Build()
    {
        if (!_moduleBuilder.IsRegistered(typeof(GameKitConfig)))
        {
            _moduleBuilder.RegisterInstance(new GameKitConfig());
        }

        _moduleBuilder.RegisterType<GameKitFactory>();

        _moduleBuilder.RegisterFunc<Window>(sp => sp.GetRequiredService<GameKitFactory>().CreateWindow(
            sp.GetRequiredService<GpuDevice>(),
            sp.GetRequiredService<AppConfig>()
        )).As<IWindow>();

        _moduleBuilder.RegisterFunc<GpuDevice>(sp => sp.GetRequiredService<GameKitFactory>().CreateGpuDevice()).As<IGpuDevice>();
        
        _moduleBuilder.RegisterType<GpuMemorySystem>();
        
        _moduleBuilder.RegisterFunc<KeyboardService>(sp => sp.GetRequiredService<GameKitFactory>().CreateKeyboardService(
            sp.GetRequiredService<AppControl>())
        ).As<IKeyboardService>();
        _moduleBuilder.RegisterFunc<GamepadService>(sp => sp.GetRequiredService<GameKitFactory>().CreateGamepadService()).As<IGamepadService>();
        _moduleBuilder.RegisterFunc<MouseService>(sp => sp.GetRequiredService<GameKitFactory>().CreateMouseService()).As<IMouseService>();
        _moduleBuilder.RegisterFunc<EventService>(sp => sp.GetRequiredService<GameKitFactory>().CreateEventService(
            sp.GetRequiredService<KeyboardService>(),
            sp.GetRequiredService<GamepadService>(),
            sp.GetRequiredService<MouseService>(),
            sp.GetRequiredService<AppControl>()
        ));
        _moduleBuilder.RegisterType<ShaderMetadataLoader>().As<IContentLoader<ShaderMetadata>>();
        
        _moduleBuilder.RegisterFunc<ShaderLoader>(sp => new ShaderLoader(
            sp.GetRequiredService<GpuDevice>(),
            sp.GetRequiredService<IContentLoader<ShaderMetadata>>(),
            sp.GetRequiredService<VirtualFileSystem>()
        )).As<IContentLoader<Shader>>();
        
        _moduleBuilder.RegisterType<TextureLoader>().As<ITextureLoader>();

        _moduleBuilder.RegisterFunc<GraphicsPipelineBuilder>(sp => new GraphicsPipelineBuilder(
            sp.GetRequiredService<GpuDevice>(),
            sp.GetRequiredService<IWindow>(),
            sp.GetRequiredService<ShaderLoader>()
        ));
        _moduleBuilder.RegisterFunc<GameKitFrameContext>(sp => sp.GetRequiredService<GameKitFactory>().CreateFrameContext()).As<FrameContext>();
        
        _moduleBuilder.RegisterFunc<FontSystem>(sp => FontSystem.Create(
            sp.GetMandatoryService<GpuMemorySystem>(),
            sp.GetMandatoryService<VirtualFileSystem>()
        )).As<IFontSystem>();

        _moduleBuilder.RegisterType<AppControl>();
        _moduleBuilder.RegisterFunc<VirtualFileSystem>(_ => _fileSystemBuilder.Create());

        _moduleBuilder.RegisterType<UpdateSystem>();
        _moduleBuilder.RegisterType<TimerSystem>();
        _moduleBuilder.RegisterType<BackgroundJobWorkerPool>();

        if (!_moduleBuilder.IsRegistered(typeof(IContentLoader<Image>)))
        {
            _moduleBuilder.RegisterType<NullImageLoader>().As<IContentLoader<Image>>();
        }

        if (_processorRegistrations.Count > 0)
        {
            _moduleBuilder.OnStart(sp =>
            {
                BackgroundJobWorkerPool pool = sp.GetRequiredService<BackgroundJobWorkerPool>();
                foreach (var registration in _processorRegistrations)
                {
                    registration(sp, pool);
                }
            });
        }

        IServiceProvider serviceProvider = _moduleBuilder.Build();

        return new GameKitApp(serviceProvider, _startables, _updatables, _disposables);
    }

    public GameKitAppBuilder RegisterAs<TImplementation, TService>()
        where TImplementation : class, TService
        where TService : class
    {
        _moduleBuilder.RegisterType<TImplementation>().As<TService>();
        return this;
    }

    public GameKitAppBuilder OnStart(Action<IServiceProvider> action)
    {
        _moduleBuilder.OnStart(action);
        return this;
    }

    public GameKitAppBuilder OnStart(Delegate action)
    {
        _moduleBuilder.OnStart(action);
        return this;
    }

    public GameKitAppBuilder RegisterBackgroundJobProcessor<TTask, TResult, TFactory>()
        where TTask : class
        where TResult : class
        where TFactory : class, IProcessorFactory<TTask, TResult>
    {
        _moduleBuilder.RegisterType<TFactory>();

        _processorRegistrations.Add((sp, pool) =>
        {
            TFactory factory = sp.GetRequiredService<TFactory>();
            pool.RegisterProcessor<TTask, TResult>(factory.Create);
        });

        return this;
    }
}
