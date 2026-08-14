using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Shaders;

namespace GameKit.Tutorials.MultiWindow;

public sealed class SecondaryWindowRenderer : IRenderPhase<SecondaryRenderContext>
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

    public void Render(SecondaryRenderContext renderContext)
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
        Window<SecondaryWindow> window,
        GraphicsPipelineBuilder graphicsPipelineBuilder,
        GpuMemorySystem gpuMemorySystem)
    {
        GpuVertexBuffer<PositionVertex> vertexBuffer =
            gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad);

        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaderProgram("shaders/shader")
            .AddColorFormatFromDisplay(window)
            .Build();

        return new SecondaryWindowRenderer(graphicsPipeline, vertexBuffer);
    }
}
