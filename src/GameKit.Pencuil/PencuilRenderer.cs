using System.Numerics;
using GameKit.Gpu;
using GameKit.Shaders;

namespace GameKit.Pencuil;

public class PencuilRenderer
{
    private readonly GpuVertexBuffer<PositionTextureVertex> _vertexBuffer;
    private readonly GraphicsPipeline _colorPipeline;
    private readonly Matrix4x4 _viewProjection;

    public PencuilRenderer(GraphicsPipelineBuilder graphicsPipelineBuilder, GpuMemorySystem gpuMemorySystem, ShaderLoader shaderLoader, AppConfig appConfig)
    {
        ReadOnlySpan<PositionTextureVertex> quad =
        [
            new(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0, 0)),
            new(new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1, 0)),
            new(new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0, 1)),
            new(new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1, 1)),
        ];

        _vertexBuffer = gpuMemorySystem.CreateVertexBuffer(quad);

        Shader vertexShader = shaderLoader.Load("shaders/pencuil_vertex");
        Shader fragmentShader = shaderLoader.Load("shaders/pencuil_color_fragment");

        _colorPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfigBasedOnBuffer(_vertexBuffer)
            .SetShaders(vertexShader, fragmentShader)
            .AddColorFormatFromDisplay(BlendingState.Standard)
            .SetCullMode(CullMode.None)
            .Build();

        uint width = appConfig.Size?.Width ?? 1280;
        uint height = appConfig.Size?.Height ?? 720;
        _viewProjection = Matrix4x4.CreateOrthographicOffCenterLeftHanded(0, width, height, 0, 0, 1);
    }

    public void Render(CommandBuffer commandBuffer, SwapchainTexture swapchainTexture, GuiContext guiContext)
    {
        if (guiContext._coloredRectangleInstructions.Count == 0 && guiContext._textureRegionInstructions.Count == 0)
        {
            guiContext.ClearInstructions();
            return;
        }

        commandBuffer.PushVertexUniformData(0, _viewProjection);

        using IRenderPass renderPass = new RenderPassBuilder(commandBuffer)
            .AddColorTarget(swapchainTexture, new ColorTargetSettings { ClearColorValue = FColors.Black })
            .Build();

        renderPass.BindGraphicsPipeline(_colorPipeline);
        renderPass.BindVertexBuffer(_vertexBuffer);

        foreach (var instruction in guiContext._coloredRectangleInstructions)
        {
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(
                instruction.Area.Width,
                instruction.Area.Height,
                1.0f);

            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(
                instruction.Area.X,
                instruction.Area.Y,
                0.0f);

            Matrix4x4 worldMatrix = scaleMatrix * translationMatrix;

            commandBuffer.PushVertexUniformData(1, worldMatrix);
            commandBuffer.PushFragmentUniformData(0, (FColor)instruction.Color);

            renderPass.DrawPrimitive();
        }

        guiContext.ClearInstructions();
    }
}
