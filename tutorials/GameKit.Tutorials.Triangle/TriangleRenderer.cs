using GameKit.Common;
using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Shaders;

namespace GameKit.Tutorials.Triangle;

public class TriangleRenderer: IRenderer<DefaultRenderContext>
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly GpuVertexBuffer<PositionTextureVertex> _quadVertexBuffer;

    public TriangleRenderer(GraphicsPipeline graphicsPipeline, GpuVertexBuffer<PositionTextureVertex> quadVertexBuffer)
    {
        _graphicsPipeline = graphicsPipeline;
        _quadVertexBuffer = quadVertexBuffer;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        using IRenderPass renderPass = renderContext
            .RenderPassBuilder
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();
        
        renderPass.BindGraphicsPipeline(_graphicsPipeline);
        renderPass.BindVertexBuffer(_quadVertexBuffer);
        
        // renderPass is disposed and rendered
    }

    public static IRenderer<DefaultRenderContext> Create(IServiceProvider serviceProvider)
    {
        ShaderLoader shaderLoader = serviceProvider.GetMandatoryService<ShaderLoader>();
        GraphicsPipelineBuilder graphicsPipelineBuilder = serviceProvider.GetMandatoryService<GraphicsPipelineBuilder>();
        GpuMemorySystem gpuMemorySystem = serviceProvider.GetMandatoryService<GpuMemorySystem>();

        GpuVertexBuffer<PositionTextureVertex> quadVertexBuffer = gpuMemorySystem.CreateVertexBuffer(PositionTextureShapes.VerticalQuad);
        
        Shader vertexShader = shaderLoader.Load("shaders/vertex");
        Shader fragmentShader = shaderLoader.Load("shaders/fragment");
        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleList)
            .AddVertexBufferConfig<PositionColorVertex>()
            .SetShaders(vertexShader, fragmentShader)
            .AddColorFormatFromDisplay()
            .Build();

        return new TriangleRenderer(graphicsPipeline, quadVertexBuffer);
    }
}