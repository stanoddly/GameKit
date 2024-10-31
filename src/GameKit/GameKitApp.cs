using GameKit.Content;
using GameKit.Input;
using SDL;

namespace GameKit;

public sealed class GameKitApp: IDisposable
{
    public required Window Window { get; init; }
    public required GpuDevice GpuDevice { get; init; }
    public required ShaderLoader ShaderLoader { get; init; }
    public required GpuMemoryUploader GpuMemoryUploader { get; init; }
    public required GraphicsPipelineBuilder GraphicsPipelineBuilder { get; init; }
    public required VirtualFileSystem FileSystem { get; init; }
    public required IContentManager ContentManager { get; init; }
    public required InputService Input { get; init; }
    public required EventService Events { get; init; }
    public required AppControl AppControl { get; init; }
    
    private Action<GameKitApp> _update = _ => { };
    private Action<GameKitApp> _draw = _ => { };

    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_ReleaseWindowFromGPUDevice(GpuDevice.SdlGpuDevice, Window.Pointer);
        }
        GpuDevice.Dispose();
        Window.Dispose();
    }

    public void Update(Action<GameKitApp> update)
    {
        _update = update;
    }

    public void Draw(Action<GameKitApp> draw)
    {
        _draw = draw;
    }

    public int Run()
    {
        while (true)
        {
            Events.ProcessEvents();

            if (AppControl.QuitRequested)
            {
                return 0;
            }
            
            _update(this);
            _draw(this);
        }
    }
}

public static class GameKitExtensions
{
    public static Shader LoadShader(this GameKitApp gameKitApp, string path)
    {
        ShaderPack shaderPack = gameKitApp.ContentManager.Load<ShaderPack>(path);
        return gameKitApp.ShaderLoader.Load(shaderPack);
    }
}