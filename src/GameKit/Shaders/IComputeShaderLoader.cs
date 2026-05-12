using GameKit.Gpu;

namespace GameKit.Shaders;

public interface IComputeShaderLoader
{
    ComputeShader Load(ReadOnlySpan<char> path);
}
