using GameKit.Content;
using GameKit.DependencyInjection;
using GameKit.Encs;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Shaders;
using GameKit.Text;

namespace GameKit.App;

public class GameKitAppBuilder : ServiceCollection
{
    private readonly FileSystemBuilder _fileSystemBuilder = new();
    private readonly List<IStartable> _startables = new();
    private readonly List<IUpdatable> _updatables = new();

    public GameKitAppBuilder()
    {
        EventBus eventBus = new();
        OnActivation(obj =>
        {
            if (obj is IStartable startable)
            {
                _startables.Add(startable);
            }
            if (obj is IUpdatable updatable)
            {
                _updatables.Add(updatable);
            }

            eventBus.Subscribe(obj);
        });

        AddSingleton(eventBus);
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
        if (!IsRegistered<GameKitConfig>())
        {
            AddSingleton(new GameKitConfig());
        }
        if (!IsRegistered<AppConfig>())
        {
            AddSingleton(new AppConfig());
        }

        AddSingleton<GameKitFactory>();

        AddSingleton<Window>((GameKitFactory factory, GpuDevice gpu, AppConfig config) => factory.CreateWindow(gpu, config));
        AddAlias<IWindow, Window>();

        AddSingleton<GpuDevice>((GameKitFactory factory) => factory.CreateGpuDevice());
        AddAlias<IGpuDevice, GpuDevice>();

        AddSingleton<GpuMemorySystem>();

        AddSingleton<KeyboardService>((GameKitFactory factory, AppControl appControl) => factory.CreateKeyboardService(appControl));
        AddAlias<IKeyboardService, KeyboardService>();

        AddSingleton<GamepadService>((GameKitFactory factory) => factory.CreateGamepadService());
        AddAlias<IGamepadService, GamepadService>();

        AddSingleton<MouseService>((GameKitFactory factory) => factory.CreateMouseService());
        AddAlias<IMouseService, MouseService>();

        AddSingleton<EventService>((GameKitFactory factory, KeyboardService keyboard, GamepadService gamepad, MouseService mouse, Window window, AppControl appControl) =>
            factory.CreateEventService(keyboard, gamepad, mouse, window, appControl));

        AddSingleton<IContentLoader<ShaderMetadata>, ShaderMetadataLoader>();

        AddSingleton<ShaderLoader>((GpuDevice gpuDevice, IContentLoader<ShaderMetadata> shaderMetadataLoader, VirtualFileSystem virtualFileSystem) =>
            new ShaderLoader(gpuDevice, shaderMetadataLoader, virtualFileSystem));
        AddAlias<IContentLoader<Shader>, ShaderLoader>();

        AddSingleton<ITextureLoader, TextureLoader>();

        AddSingleton<GraphicsPipelineBuilder>((GpuDevice gpuDevice, IWindow window, IContentLoader<Shader> shaderLoader) =>
            new GraphicsPipelineBuilder(gpuDevice, window, shaderLoader));

        AddSingleton<GameKitFrameContext>((GameKitFactory factory) => factory.CreateFrameContext());
        AddAlias<FrameContext, GameKitFrameContext>();

        AddSingleton<FontSystem>(FontSystem.Create);
        AddAlias<IFontSystem, FontSystem>();

        AddSingleton<AppControl>();
        AddSingleton<VirtualFileSystem>(() => _fileSystemBuilder.Create());

        AddSingleton<UpdateSystem>();
        AddAlias<ITickRegistrar, UpdateSystem>();
        AddSingleton<TimerSystem>();

        if (!IsRegistered<IContentLoader<Image>>())
        {
            AddSingleton<IContentLoader<Image>, SdlImageLoader>();
        }

        ServiceProvider serviceProvider = BuildServiceProvider();
        return new GameKitApp(serviceProvider, _startables, _updatables);
    }
}
