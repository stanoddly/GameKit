using GameKit.Content;
using GameKit.DependencyInjection;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Shaders;
using GameKit.Text;

namespace GameKit.App;

public class GameKitAppBuilder : ServiceCollection
{
    private readonly FileSystemBuilder _fileSystemBuilder = new();

    public GameKitAppBuilder()
    {
        WireUpdatableLifecycle();
    }

    private void WireUpdatableLifecycle()
    {
        UpdateLoop updateLoop = new();
        AddSingleton(updateLoop);
        OnActivated((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateLoop.Register(updatable);
            }
        });

        OnDisposing((instance, _) =>
        {
            if (instance is IUpdatable updatable)
            {
                updateLoop.Unregister(updatable);
            }
        });
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

        AddSingleton<PlatformInfo, GameKitFactory>();

        AddSingleton<Window, GameKitFactory>();

        AddSingleton<GpuDevice, GameKitFactory>();

        AddSingleton<GpuMemorySystem>();

        AddSingleton<KeyboardService, GameKitFactory>();
        AddAlias<IKeyboardService, KeyboardService>();

        AddSingleton<GamepadService, GameKitFactory>();
        AddAlias<IGamepadService, GamepadService>();

        AddSingleton<MouseService, GameKitFactory>();
        AddAlias<IMouseService, MouseService>();

        AddSingleton<TextInputService, GameKitFactory>();
        AddAlias<ITextInputService, TextInputService>();

        AddSingleton<ClipboardService>();
        AddAlias<IClipboardService, ClipboardService>();

        AddSingleton<EventService, GameKitFactory>();

        AddSingleton<GraphicsShaderMetadataLoader>();

        AddSingleton<ShaderLoader>();
        AddAlias<IShaderLoader, ShaderLoader>();

        AddSingleton<ITextureLoader, TextureLoader>();

        AddSingleton<GraphicsPipelineBuilder>();

        AddSingleton<ComputeShaderMetadataLoader>();

        AddSingleton<ComputeShaderLoader>();
        AddAlias<IComputeShaderLoader, ComputeShaderLoader>();

        AddSingleton<ComputePipelineBuilder>();

        AddSingleton<GameKitFrameContext>();
        AddAlias<FrameContext, GameKitFrameContext>();

        AddSingleton<FontSystem>(FontSystem.Create);
        AddAlias<IFontSystem, FontSystem>();

        AddSingleton<AppControl>();
        AddSingleton<VirtualFileSystem>(() => _fileSystemBuilder.Create());

        AddSingleton<UpdateSystem>();
        AddSingleton<TimerSystem>();

        AddSingleton<StageManager>();
        AddAlias<IStageManager, StageManager>();

        if (!IsRegistered<IImageLoader>())
        {
            AddSingleton<IImageLoader, SdlImageLoader>();
        }

        ServiceProvider serviceProvider = BuildServiceProvider();
        return new GameKitApp(serviceProvider);
    }
}
