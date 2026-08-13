using GameKit.Gpu;

namespace GameKit.Shaders;

public interface IShaderLoader
{
    GraphicsShaderProgram LoadGraphicsShaderProgram(ReadOnlySpan<char> path);
}
