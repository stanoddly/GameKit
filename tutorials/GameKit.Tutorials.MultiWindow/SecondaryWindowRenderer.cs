using GameKit.Gpu;
using GameKit.RenderOrchestration;

namespace GameKit.Tutorials.MultiWindow;

public class SecondaryWindowRenderer : IRenderer<SecondaryRenderContext>
{
    private readonly GraphicsPipelineBuilder _graphicsPipelineBuilder;
    private readonly GpuVertexBuffer<PositionVertex> _vertexBuffer;
    private GraphicsPipeline? _graphicsPipeline;

    public SecondaryWindowRenderer(
        GraphicsPipelineBuilder graphicsPipelineBuilder,
        GpuVertexBuffer<PositionVertex> vertexBuffer)
    {
        _graphicsPipelineBuilder = graphicsPipelineBuilder;
        _vertexBuffer = vertexBuffer;
    }

    public void Render(SecondaryRenderContext renderContext)
    {
        if (_graphicsPipeline == null)
        {
            _graphicsPipeline = _graphicsPipelineBuilder
                .SetPrimitiveType(PrimitiveType.TriangleStrip)
                .AddVertexBufferConfig<PositionVertex>()
                .SetShaderProgram("shaders/shader")
                .AddColorFormatFromDisplay<SecondaryRenderContext>()
                .Build();
        }

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
        GpuMemorySystem gpuMemorySystem)
    {
        GpuVertexBuffer<PositionVertex> vertexBuffer = gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad);
        return new SecondaryWindowRenderer(graphicsPipelineBuilder, vertexBuffer);
    }
}
