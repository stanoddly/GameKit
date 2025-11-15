using System.Runtime.InteropServices;
using GameKit.Shaders;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct UniformSlotSizes(byte Slot0, byte Slot1, byte Slot2, byte Slot3);

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
public readonly record struct ShaderBindingCounts(UniformSlotSizes UniformSlotSizes, byte StorageTexturesCount, byte StorageBuffersCount, byte UniformBuffersCount);

public class Shader: IDisposable
{
    private readonly GpuDevice _gpuDevice;
    internal Pointer<SDL_GPUShader> Pointer { get; set; }
    public ShaderStage Stage { get; }
    public ShaderBindingCounts BindingCounts { get; }

    internal Shader(GpuDevice gpuDevice, Pointer<SDL_GPUShader> pointer, ShaderStage stage, ShaderBindingCounts shaderBindingCounts)
    {
        _gpuDevice = gpuDevice;
        Pointer = pointer;
        Stage = stage;

        BindingCounts = shaderBindingCounts;
    }

    public void Dispose()
    {
        _gpuDevice.ReleaseShader(this);
    }
}