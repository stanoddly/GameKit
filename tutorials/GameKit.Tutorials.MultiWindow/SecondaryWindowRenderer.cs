using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

public sealed class SecondaryWindowRenderer : IRenderPhase<DefaultRenderContext>
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly GpuVertexBuffer<PositionVertex> _vertexBuffer;

    public SecondaryWindowRenderer(
        GraphicsPipeline graphicsPipeline,
        GpuVertexBuffer<PositionVertex> vertexBuffer)
    {
        _graphicsPipeline = graphicsPipeline;
        _vertexBuffer = vertexBuffer;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        renderContext.CommandBuffer.PushFragmentUniformData(0, FColors.Coral);
        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();

        renderPass.BindGraphicsPipeline(_graphicsPipeline);
        renderPass.BindVertexBuffer(_vertexBuffer);
        renderPass.DrawPrimitive();
    }

    public static SecondaryWindowRenderer Create(
        GraphicsPipelineBuilder graphicsPipelineBuilder,
        GpuVertexBuffer<PositionVertex> vertexBuffer)
    {
        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaders("shaders/vertex", "shaders/fragment")
            .AddColorFormatFromDisplay()
            .Build();

        return new SecondaryWindowRenderer(graphicsPipeline, vertexBuffer);
    }
}
