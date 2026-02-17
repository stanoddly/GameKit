using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Represents the default rendering context, holding resources required for a single frame rendering.
/// </summary>
public class DefaultRenderContext: IRenderContext
{
    /// <summary>
    /// The swapchain texture for the current frame.
    /// </summary>
    public SwapchainTexture SwapchainTexture { get; }

    /// <summary>
    /// The command buffer used to record rendering commands.
    /// </summary>
    public CommandBuffer CommandBuffer { get; }

    /// <inheritdoc />
    public virtual Texture ColorTarget => SwapchainTexture;

    public DefaultRenderContext(SwapchainTexture swapchainTexture, CommandBuffer commandBuffer)
    {
        SwapchainTexture = swapchainTexture;
        CommandBuffer = commandBuffer;
    }

    public virtual void Dispose()
    {
        CommandBuffer.Submit();
    }
}