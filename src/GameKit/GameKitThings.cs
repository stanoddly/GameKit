using GameKit.Gpu;
using GameKit.Shaders;
using GameKit.Utilities;
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

public class Shader: IDisposable
{
    private readonly GpuDevice _gpuDevice;
    internal Pointer<SDL_GPUShader> Pointer { get; set; }
    public ShaderStage Stage { get; }
    public int SamplersCount { get; }
    public int StorageTexturesCount { get; }
    public int StorageBuffersCount { get; }
    public int UniformBuffersCount { get; }

    internal Shader(GpuDevice gpuDevice, Pointer<SDL_GPUShader> pointer, ShaderStage stage, int samplersCount, int storageTexturesCount, int storageBuffersCount, int uniformBuffersCount)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
        Stage = stage;
        SamplersCount = samplersCount;
        StorageTexturesCount = storageTexturesCount;
        StorageBuffersCount = storageBuffersCount;
        UniformBuffersCount = uniformBuffersCount;
    }

    public void Dispose()
    {
        _gpuDevice.ReleaseShader(this);
    }
}