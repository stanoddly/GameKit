using GameKit.Gpu;

namespace GameKit.Shaders;

public interface IShaderLoader
{
    VertexShader LoadVertexShader(ReadOnlySpan<char> path);
    FragmentShader LoadFragmentShader(ReadOnlySpan<char> path);
}
