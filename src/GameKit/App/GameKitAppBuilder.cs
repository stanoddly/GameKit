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
        UpdateLoop updateLoop = new();
        AddSingleton(updateLoop);
        this.RegisterUpdatables(updateLoop);
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

        AddSingleton<Window>((GameKitFactory factory, GpuDevice gpu, AppConfig config, GameKitFrameContext frameContext) => factory.CreateWindow(gpu, frameContext, config));
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

        AddSingleton<TextInputService>((GameKitFactory factory, Window window) => factory.CreateTextInputService(window));
        AddAlias<ITextInputService, TextInputService>();

        AddSingleton<ClipboardService>();
        AddAlias<IClipboardService, ClipboardService>();

        AddSingleton<EventService>((GameKitFactory factory, KeyboardService keyboard, GamepadService gamepad, MouseService mouse, TextInputService textInput, Window window, AppControl appControl) =>
            factory.CreateEventService(keyboard, gamepad, mouse, textInput, window, appControl));

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

        if (!IsRegistered<IImageLoader>())
        {
            AddSingleton<IImageLoader, SdlImageLoader>();
        }

        ServiceProvider serviceProvider = BuildServiceProvider();
        return new GameKitApp(serviceProvider);
    }
}
