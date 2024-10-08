using System.Numerics;
using System.Reflection;
using SDL;

namespace GameKit;

public class GameKitInitializationException : Exception
{
    public GameKitInitializationException()
    {
    }

    public GameKitInitializationException(string message)
        : base(message)
    {
    }

    public GameKitInitializationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public struct Window: IDisposable
{
    public Pointer<SDL_Window> Pointer { get; private set; }

    public Window(Pointer<SDL_Window> sdlWindow)
    {
        Pointer = sdlWindow;
    }

    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_DestroyWindow(Pointer);
            Pointer = null;
        }
    }
}

public struct GpuCommandBuffer
{
    public Pointer<SDL_GPUCommandBuffer> Pointer { get; private set; }

    public GpuCommandBuffer(Pointer<SDL_GPUCommandBuffer> sdlCommandBuffer)
    {
        Pointer = sdlCommandBuffer;
    }
}

public class Shader
{
    public Pointer<SDL_GPUShader> Pointer { get; }
    public ShaderStage Stage { get; }
    public int SamplersCount { get; }
    public int StorageTexturesCount { get; }
    public int StorageBuffersCount { get; }
    public int UniformBuffersCount { get; }

    public Shader(Pointer<SDL_GPUShader> pointer, ShaderStage stage, int samplersCount, int storageTexturesCount, int storageBuffersCount, int uniformBuffersCount)
    {
        Pointer = pointer;
        Stage = stage;
        SamplersCount = samplersCount;
        StorageTexturesCount = storageTexturesCount;
        StorageBuffersCount = storageBuffersCount;
        UniformBuffersCount = uniformBuffersCount;
    }
}
/*
public struct PositionTextureVertex
{
    Vector3 Position;
    Vector2 TexCoord;
}
*/
public struct RenderingPipeline<TVertexType> where TVertexType: unmanaged, IVertexType
{
    public Pointer<SDL_GPUGraphicsPipeline> Pointer { get; private set; }

    public RenderingPipeline(Pointer<SDL_GPUGraphicsPipeline> pointer)
    {
        Pointer = pointer;
    }
}

public struct RenderingPipeline
{
    public Pointer<SDL_GPUGraphicsPipeline> Pointer { get; private set; }

    public RenderingPipeline(Pointer<SDL_GPUGraphicsPipeline> pointer)
    {
        Pointer = pointer;
    }
}

public struct GameKitContextBuilder
{
#if DEBUG
    private const bool DebugBuild = true;
#else
    private const bool DebugBuild = false;
#endif

    private static readonly string DefaultTitle = "GameKit App";
    private static readonly (int, int) DefaultSize = (640, 480);
    private (int, int)? _windowSize;
    private string? _windowTitle;
    private bool? _debugMode;
    

    public GameKitContext Build()
    {
        return UnsafeBuild();
    }

    public GameKitContextBuilder WithSize((int, int) size)
    {
        _windowSize = size;
        return this;
    }

    public GameKitContextBuilder WithTitle(string title)
    {
        _windowTitle = title;
        return this;
    }

    public GameKitContextBuilder WithDebugMode()
    {
        _debugMode = true;
        return this;
    }

    public GameKitContextBuilder WithRootDirectoryFromExecutable(string? subdirectory = null)
    {
        string? executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // no need to change anything
        if (executableDirectory == null && subdirectory == null)
        {
            return this;
        }
        
        if (subdirectory == null)
        {
            Directory.SetCurrentDirectory(executableDirectory!);
            return this;
        }

        // we are at root
        if (executableDirectory == null)
        {
            Directory.SetCurrentDirectory(subdirectory!);
            return this;
        }
        
        Directory.SetCurrentDirectory(Path.Combine(executableDirectory, "Content"));
        return this;
    }

    private unsafe GameKitContext UnsafeBuild()
    {
        if (SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO) == SDL_bool.SDL_FALSE)
        {
            throw new GameKitInitializationException($"SDL_Init failed: {SDL3.SDL_GetError()}");
        }
        
        Pointer<SDL_GPUDevice> device = SDL3.SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV, SDL_bool.SDL_TRUE, (byte*)null);
        if (device.IsNull())
        {
            throw new GameKitInitializationException($"SDL_CreateGPUDevice failed: {SDL3.SDL_GetError()}");
        }

        string windowTitle;
        if (_windowTitle == null)
        {
            using var process = System.Diagnostics.Process.GetCurrentProcess();
            windowTitle = process.ProcessName;
        }
        else
        {
            windowTitle = _windowTitle;
        }
        
        (int width, int height) = _windowSize ?? DefaultSize;
        Pointer<SDL_Window> window = SDL3.SDL_CreateWindow(windowTitle, width, height, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        if (window.IsNull())
        {
            throw new GameKitInitializationException($"SDL_CreateWindow failed: {SDL3.SDL_GetError()}");
        }

        if (SDL3.SDL_ClaimWindowForGPUDevice(device, window) == SDL_bool.SDL_FALSE)
        {
            throw new GameKitInitializationException($"GPUClaimWindow failed: {SDL3.SDL_GetError()}");
        }
        
        return new GameKitContext {GpuDevice = new GpuDevice(device, window), Window = new Window(window)};
    }
}