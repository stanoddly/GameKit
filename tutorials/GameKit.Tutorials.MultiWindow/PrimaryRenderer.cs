using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Shaders;

namespace GameKit.Tutorials.MultiWindow;

public class PrimaryRenderer : IRenderPhase<DefaultRenderContext>
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly GpuVertexBuffer<PositionVertex> _vertexBuffer;

    public PrimaryRenderer(GraphicsPipeline graphicsPipeline, GpuVertexBuffer<PositionVertex> vertexBuffer)
    {
        _graphicsPipeline = graphicsPipeline;
        _vertexBuffer = vertexBuffer;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        renderContext.CommandBuffer.PushFragmentUniformData(0, FColors.SkyBlue);
        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();

        renderPass.BindGraphicsPipeline(_graphicsPipeline);
        renderPass.BindVertexBuffer(_vertexBuffer);
        renderPass.DrawPrimitive();
    }

    public static PrimaryRenderer Create(ShaderLoader shaderLoader, GraphicsPipelineBuilder graphicsPipelineBuilder, GpuMemorySystem gpuMemorySystem)
    {
        GpuVertexBuffer<PositionVertex> vertexBuffer = gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad);

        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaderProgram("shaders/shader")
            .AddColorFormatFromDisplay()
            .Build();

        return new PrimaryRenderer(graphicsPipeline, vertexBuffer);
    }
}
