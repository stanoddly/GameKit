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

public struct GraphicsPipeline
{
    public Pointer<SDL_GPUGraphicsPipeline> Pointer { get; private set; }

    public GraphicsPipeline(Pointer<SDL_GPUGraphicsPipeline> pointer)
    {
        Pointer = pointer;
    }
}
