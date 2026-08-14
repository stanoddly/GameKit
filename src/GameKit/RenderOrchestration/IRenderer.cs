using GameKit.Gpu;

namespace GameKit.RenderOrchestration;

public interface IRenderer
{
    void Render(CommandBuffer commandBuffer, IRenderPass screenRenderPass);
}
