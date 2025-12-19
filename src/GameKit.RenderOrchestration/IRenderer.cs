using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

/// <summary>
/// Defines a renderer that can draw to a command buffer within a given render pass.
/// </summary>
public interface IRenderer
{
    /// <summary>
    /// Records rendering commands into the provided command buffer.
    /// </summary>
    /// <param name="commandBuffer">The command buffer to record commands into.</param>
    /// <param name="screenRenderPass">The render pass to use for rendering.</param>
    void Render(CommandBuffer commandBuffer, IRenderPass screenRenderPass);
}