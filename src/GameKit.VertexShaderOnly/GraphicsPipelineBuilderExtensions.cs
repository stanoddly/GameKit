using GameKit.Gpu;
using GameKit.Shaders;

namespace GameKit.VertexShaderOnly;

/// <summary>
/// Extension methods for <see cref="GraphicsPipelineBuilder"/> to create vertex-shader-only pipelines.
/// </summary>
public static class GraphicsPipelineBuilderExtensions
{
    private const string NoopFragmentShaderPath = "shaders/noop";

    /// <summary>
    /// Sets only the vertex shader for the pipeline, using an internal no-op fragment shader.
    /// This enables depth-only rendering for shadow mapping and similar techniques.
    /// </summary>
    /// <param name="builder">The graphics pipeline builder.</param>
    /// <param name="vertexShader">The vertex shader to use.</param>
    /// <returns>The graphics pipeline builder for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the no-op fragment shader is not found. Ensure the VertexShaderOnly embedded file system
    /// is included in the VirtualFileSystem.
    /// </exception>
    public static GraphicsPipelineBuilder SetVertexShader(
        this GraphicsPipelineBuilder builder,
        Shader vertexShader)
    {
        Shader fragmentShader;
        try
        {
            fragmentShader = builder.ShaderLoader.Load(NoopFragmentShaderPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load the no-op fragment shader at '{NoopFragmentShaderPath}'. " +
                "Ensure the VertexShaderOnly embedded file system is included in the VirtualFileSystem.",
                ex);
        }

        return builder.SetShaders(vertexShader, fragmentShader);
    }
}
