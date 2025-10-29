using SDL;

namespace GameKit.Gpu;

/// <summary>
/// Defines how polygons are rendered.
/// </summary>
public class RasterizerState
{
    /// <summary>
    /// Gets or sets the culling mode for polygons.
    /// </summary>
    public CullMode CullMode { get; set; }
    
    /// <summary>
    /// Gets or sets which face is considered the front face for culling.
    /// </summary>
    public FrontFace FrontFace { get; set; }
}

/// <summary>
/// Defines the polygon culling mode.
/// </summary>
public enum CullMode
{
    /// <summary>
    /// Do not cull any polygons.
    /// </summary>
    None = SDL_GPUCullMode.SDL_GPU_CULLMODE_NONE,
    
    /// <summary>
    /// Cull front-facing polygons.
    /// </summary>
    Front = SDL_GPUCullMode.SDL_GPU_CULLMODE_FRONT,
    
    /// <summary>
    /// Cull back-facing polygons.
    /// </summary>
    Back = SDL_GPUCullMode.SDL_GPU_CULLMODE_BACK
}

/// <summary>
/// Defines which winding order is considered the front face.
/// </summary>
public enum FrontFace
{
    /// <summary>
    /// Counter-clockwise winding order defines front-facing polygons.
    /// </summary>
    CounterClockwise = SDL_GPUFrontFace.SDL_GPU_FRONTFACE_COUNTER_CLOCKWISE,
    
    /// <summary>
    /// Clockwise winding order defines front-facing polygons.
    /// </summary>
    Clockwise = SDL_GPUFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE
}
