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
public abstract partial class GameKitModule
{
    // Consumer-provided
    public abstract AppConfig AppConfig { get; }
    public abstract GameKitConfig GameKitConfig { get; }
    public abstract VirtualFileSystem FileSystem { get; }

    // Lifecycle tracking
    public List<IUpdatable> Updatables { get; } = new();
    public EventBus EventBus { get; } = new();

    [OnActivate]
    protected void TrackUpdatable(IUpdatable updatable)
    {
        Updatables.Add(updatable);
    }

    [OnActivate]
    protected void SubscribeEventBus(object obj)
    {
        EventBus.Subscribe(obj);
    }

    // Framework services
    [Singleton]
    public partial AppControl AppControl { get; }

    [Singleton]
    public partial GameKitFactory GameKitFactory { get; }

    [Singleton]
    public partial UpdateSystem UpdateSystem { get; }

    [Singleton]
    public partial TimerSystem TimerSystem { get; }

    // Factory-created via GameKitFactory
    [Singleton, Factory<GameKitFactory>]
    public partial IGpuDevice GpuDevice { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial IWindow Window { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial IKeyboardService KeyboardService { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial IGamepadService GamepadService { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial IMouseService MouseService { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial EventService EventService { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial GameKitFrameContext FrameContext { get; }

    // Constructor-injected singletons
    [Singleton]
    public partial GpuMemorySystem GpuMemorySystem { get; }

    [Singleton]
    public partial ShaderMetadataLoader ShaderMetadataLoader { get; }

    [Singleton]
    public partial TextureLoader TextureLoader { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial IContentLoader<Shader> ShaderLoader { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial GraphicsPipelineBuilder GraphicsPipelineBuilder { get; }

    // Interface-typed singletons
    [Singleton, Factory<GameKitFactory>]
    public partial IContentLoader<Image> ImageLoader { get; }

    [Singleton, Factory<GameKitFactory>]
    public partial IFontSystem FontSystem { get; }
}
