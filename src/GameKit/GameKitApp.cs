using GameKit.Content;
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
    
    private Action<GameKitApp> _update = _ => { };
    private Action<GameKitApp> _draw = _ => { };
    
    private bool ConsumeEventsAndFalseOnQuit()
    {
        unsafe
        {
            SDL_Event evt;
            while (SDL3.SDL_PollEvent(&evt) == true)
            {
                if (evt.Type == SDL_EventType.SDL_EVENT_QUIT)
                {
                    return false;
                }
            }
        }

        return true;
    }

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
        while (ConsumeEventsAndFalseOnQuit())
        {
            _update(this);
            _draw(this);
        }

        return 0;
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