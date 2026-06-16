using System.Numerics;
using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Shaders;

namespace GameKit.Tutorials.ClickThrough;

public class ClickThroughRenderer : IRenderPhase<DefaultRenderContext>
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly GpuVertexBuffer<PositionVertex> _quad;

    public ClickThroughRenderer(GraphicsPipeline graphicsPipeline, GpuVertexBuffer<PositionVertex> quad)
    {
        _graphicsPipeline = graphicsPipeline;
        _quad = quad;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(new ColorTargetSettings
            {
                ClearColorValue = FColors.Black,
                LoadOperation = LoadOperation.Clear
            })
            .Build();

        renderPass.BindGraphicsPipeline(_graphicsPipeline);
        renderContext.CommandBuffer.PushFragmentUniformData(0, FColors.SkyBlue);
        renderPass.BindVertexBuffer(_quad);
        renderPass.DrawPrimitive();
    }

    public static ClickThroughRenderer Create(ShaderLoader shaderLoader, GraphicsPipelineBuilder graphicsPipelineBuilder, GpuMemorySystem gpuMemorySystem)
    {
        // NDC (-0.75, -0.75) to (0.75, 0.75) maps to pixels (50, 50)-(350, 350) in a 400x400 window
        ReadOnlySpan<PositionVertex> vertices =
        [
            new(new Vector3(-0.75f, -0.75f, 0.0f)),
            new(new Vector3(-0.75f,  0.75f, 0.0f)),
            new(new Vector3( 0.75f, -0.75f, 0.0f)),
            new(new Vector3( 0.75f,  0.75f, 0.0f)),
        ];

        GpuVertexBuffer<PositionVertex> quad = gpuMemorySystem.CreateVertexBuffer(vertices);

        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaders("shaders/vertex", "shaders/fragment")
            .AddColorFormatFromDisplay()
            .Build();

        return new ClickThroughRenderer(graphicsPipeline, quad);
    }
}
