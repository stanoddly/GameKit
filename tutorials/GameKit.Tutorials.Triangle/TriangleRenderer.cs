using System.Numerics;
using GameKit.Common;
using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Shaders;

namespace GameKit.Tutorials.Triangle;

public class TriangleRenderer: IRenderer<DefaultRenderContext>
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly GpuVertexBuffer<PositionVertex> _quadVertexBuffer;

    public TriangleRenderer(GraphicsPipeline graphicsPipeline, GpuVertexBuffer<PositionVertex> quadVertexBuffer)
    {
        _graphicsPipeline = graphicsPipeline;
        _quadVertexBuffer = quadVertexBuffer;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        renderContext.CommandBuffer.PushFragmentUniformData(0, FColors.Magenta);
        using IRenderPass renderPass = renderContext
            .RenderPassBuilder
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();
        
        renderPass.BindGraphicsPipeline(_graphicsPipeline);
        renderPass.BindVertexBuffer(_quadVertexBuffer);
        
        renderPass.DrawPrimitive();
        
        // renderPass is disposed and rendered
    }

    public static IRenderer<DefaultRenderContext> Create(IServiceProvider serviceProvider)
    {
        ShaderLoader shaderLoader = serviceProvider.GetMandatoryService<ShaderLoader>();
        GraphicsPipelineBuilder graphicsPipelineBuilder = serviceProvider.GetMandatoryService<GraphicsPipelineBuilder>();
        GpuMemorySystem gpuMemorySystem = serviceProvider.GetMandatoryService<GpuMemorySystem>();

        GpuVertexBuffer<PositionVertex> quadVertexBuffer = gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad);
        
        Shader vertexShader = shaderLoader.Load("shaders/vertex");
        Shader fragmentShader = shaderLoader.Load("shaders/fragment");
        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleList)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaders(vertexShader, fragmentShader)
            .AddColorFormatFromDisplay()
            .Build();

        return new TriangleRenderer(graphicsPipeline, quadVertexBuffer);
    }
}