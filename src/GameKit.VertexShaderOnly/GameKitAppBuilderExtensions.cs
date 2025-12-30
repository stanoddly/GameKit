using System.Reflection;
using GameKit.App;
using GameKit.Content;

namespace GameKit.VertexShaderOnly;

/// <summary>
/// Extension methods for <see cref="GameKitAppBuilder"/> to add vertex-shader-only pipeline support.
/// </summary>
public static class GameKitAppBuilderExtensions
{
    /// <summary>
    /// Adds support for vertex-shader-only pipelines by registering the embedded no-op fragment shader.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static GameKitAppBuilder AddVertexShaderOnlySupport(this GameKitAppBuilder builder)
    {
        VirtualFileSystem embeddedFs = EmbeddedFileSystem.Create(typeof(GameKitAppBuilderExtensions).Assembly);
        builder.AddFileSystem(embeddedFs);
        return builder;
    }
}
