using GameKit.App;
using GameKit.Common;
using GameKit.Content;
using GameKit.Encs;
using GameKit.Gpu;
using GameKit.Input;
using GameKit.Shaders;
using GameKit.Text;
using Yak;

namespace GameKit.Modules;

[Module]
public interface IGameKitCore
{
    // Consumer-provided
    AppConfig AppConfig { get; }
    GameKitConfig GameKitConfig { get; }
    VirtualFileSystem FileSystem { get; }
    EventBus EventBus { get; }

    // Lifecycle tracking (populated by [OnActivate] on concrete class or base class)
    List<IStartable> Startables { get; }
    List<IUpdatable> Updatables { get; }

    // Yak generates this on the concrete module class
    void ResolveAll();

    // Framework services
    [Singleton]
    AppControl AppControl { get; }

    [Singleton]
    GameKitFactory GameKitFactory { get; }

    [Singleton]
    UpdateSystem UpdateSystem { get; }

    [Singleton]
    TimerSystem TimerSystem { get; }

    // Factory-created via GameKitFactory
    [Singleton, Factory<GameKitFactory>]
    GpuDevice GpuDevice { get; }

    [Singleton, Factory<GameKitFactory>]
    Window Window { get; }

    [Singleton, Factory<GameKitFactory>]
    KeyboardService KeyboardService { get; }

    [Singleton, Factory<GameKitFactory>]
    GamepadService GamepadService { get; }

    [Singleton, Factory<GameKitFactory>]
    MouseService MouseService { get; }

    [Singleton, Factory<GameKitFactory>]
    EventService EventService { get; }

    [Singleton, Factory<GameKitFactory>]
    GameKitFrameContext FrameContext { get; }

    // Constructor-injected singletons
    [Singleton]
    GpuMemorySystem GpuMemorySystem { get; }

    [Singleton]
    ShaderMetadataLoader ShaderMetadataLoader { get; }

    [Singleton]
    ShaderLoader ShaderLoader { get; }

    [Singleton]
    TextureLoader TextureLoader { get; }

    [Singleton]
    GraphicsPipelineBuilder GraphicsPipelineBuilder { get; }

    // Interface-typed singletons (property type matches constructor param type)
    [Singleton<SdlImageLoader>]
    IContentLoader<Image> ImageLoader { get; }

    [Singleton<FontSystem>, StaticFactory<FontSystem>]
    IFontSystem FontSystem { get; }
}
