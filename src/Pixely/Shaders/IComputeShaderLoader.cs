using Pixely.Gpu;

namespace Pixely.Shaders;

public interface IComputeShaderLoader
{
    ComputeShader Load(ReadOnlySpan<char> path);
}
