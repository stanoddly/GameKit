using System.Numerics;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.RenderOrchestration;
using GameKit.Shaders;

namespace GameKit.Tutorials.StorageBuffer;

public class StorageBufferRenderer : IRenderPhase<DefaultRenderContext>
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly GpuVertexBuffer<PositionVertex> _quadVertexBuffer;
    private readonly GpuStorageBuffer<Vector4> _colorBuffer;
    private readonly int _colorCount;
    private float _time;

    public StorageBufferRenderer(
        GraphicsPipeline graphicsPipeline,
        GpuVertexBuffer<PositionVertex> quadVertexBuffer,
        GpuStorageBuffer<Vector4> colorBuffer,
        int colorCount)
    {
        _graphicsPipeline = graphicsPipeline;
        _quadVertexBuffer = quadVertexBuffer;
        _colorBuffer = colorBuffer;
        _colorCount = colorCount;
    }

    public void Render(DefaultRenderContext renderContext)
    {
        _time += 0.016f; // Approximate 60fps timestep

        // Cycle through colors every 0.5 seconds
        int colorIndex = (int)(_time * 2) % _colorCount;

        // Pass the index to the shader via uniform
        renderContext.CommandBuffer.PushFragmentUniformData(0, colorIndex);

        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(renderContext.SwapchainTexture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();

        renderPass.BindGraphicsPipeline(_graphicsPipeline);
        renderPass.BindVertexBuffer(0, _quadVertexBuffer);
        renderPass.BindFragmentStorageBuffer(_colorBuffer);

        renderPass.DrawPrimitive();
    }

    public static StorageBufferRenderer Create(
        ShaderLoader shaderLoader,
        GraphicsPipelineBuilder graphicsPipelineBuilder,
        GpuMemorySystem gpuMemorySystem,
        IGpuDevice gpuDevice)
    {
        // Create an array of colors to store in the storage buffer
        // This demonstrates passing more data than uniform slots would allow
        Vector4[] colors =
        [
            new Vector4(1.0f, 0.0f, 0.0f, 1.0f), // Red
            new Vector4(1.0f, 0.5f, 0.0f, 1.0f), // Orange
            new Vector4(1.0f, 1.0f, 0.0f, 1.0f), // Yellow
            new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            new Vector4(0.0f, 1.0f, 1.0f, 1.0f), // Cyan
            new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Blue
            new Vector4(0.5f, 0.0f, 1.0f, 1.0f), // Purple
            new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Magenta
        ];

        // Create storage buffer from color array
        GpuStorageBuffer<Vector4> colorBuffer = gpuMemorySystem.CreateStorageBuffer<Vector4>(colors);

        // Create vertex buffer with a quad
        GpuVertexBuffer<PositionVertex> quadVertexBuffer =
            gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad);

        // Build graphics pipeline
        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaders("shaders/vertex", "shaders/fragment")
            .AddColorFormatFromDisplay()
            .Build();

        return new StorageBufferRenderer(graphicsPipeline, quadVertexBuffer, colorBuffer, colors.Length);
    }
}
