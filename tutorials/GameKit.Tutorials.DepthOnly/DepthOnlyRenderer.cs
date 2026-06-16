using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Shaders;
using GameKit.VertexShaderOnly;

namespace GameKit.Tutorials.DepthOnly;

public class DepthOnlyRenderer : IRenderPhase<DefaultRenderContext>
{
    private readonly GraphicsPipeline _depthOnlyPipeline;
    private readonly GpuVertexBuffer<PositionVertex> _vertexBuffer;
    private readonly Texture _depthTexture;

    public DepthOnlyRenderer(
        GraphicsPipeline depthOnlyPipeline,
        GpuVertexBuffer<PositionVertex> vertexBuffer,
        Texture depthTexture)
    {
        _depthOnlyPipeline = depthOnlyPipeline;
        _vertexBuffer = vertexBuffer;
        _depthTexture = depthTexture;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        // First pass: Render to depth-only (no color target)
        using (IRenderPass depthPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .SetDepthBuffer(_depthTexture, DepthBufferSettings.Default)
            .Build())
        {
            depthPass.BindGraphicsPipeline(_depthOnlyPipeline);
            depthPass.BindVertexBuffer(_vertexBuffer);
            depthPass.DrawPrimitive();
        }

        // Second pass: Clear swapchain to green to show the app is running
        using (IRenderPass colorPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(renderContext.SwapchainTexture, new ColorTargetSettings
            {
                ClearColorValue = new FColor(0.2f, 0.6f, 0.2f, 1.0f)
            })
            .Build())
        {
            // Nothing to draw - just clearing to show success
        }
    }

    public static DepthOnlyRenderer Create(
        ShaderLoader shaderLoader,
        GraphicsPipelineBuilder graphicsPipelineBuilder,
        GpuMemorySystem gpuMemorySystem,
        GpuDevice gpuDevice)
    {
        // Create depth texture
        Texture depthTexture = gpuDevice.CreateDepthBufferTexture(
            new ShortSize(800, 600),
            DepthBufferFormat.Depth32);

        // Create vertex buffer with a simple quad
        GpuVertexBuffer<PositionVertex> vertexBuffer = gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad);

        // Load vertex shader
        VertexShader vertexShader = shaderLoader.LoadVertexShader("shaders/depth_vertex");

        // Create depth-only pipeline using the extension method
        GraphicsPipeline depthOnlyPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetVertexShader(vertexShader)  // Uses internal no-op fragment shader
            .EnableDepthTesting(DepthBufferFormat.Depth32)
            .Build();

        return new DepthOnlyRenderer(depthOnlyPipeline, vertexBuffer, depthTexture);
    }
}
