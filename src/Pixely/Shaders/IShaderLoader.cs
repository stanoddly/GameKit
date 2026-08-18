using Pixely.Gpu;

namespace Pixely.Shaders;

public interface IShaderLoader
{
    GraphicsShaderProgram LoadGraphicsShaderProgram(ReadOnlySpan<char> path);
}
