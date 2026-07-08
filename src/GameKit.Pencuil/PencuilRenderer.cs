using System.Numerics;
using GameKit.Gpu;
using GameKit.Shaders;

namespace GameKit.Pencuil;

public class PencuilRenderer
{
    private static readonly ColorTargetSettings _guiColorTargetSettings = new()
    {
        ClearColorValue = FColors.Transparent
    };

    private static readonly Matrix4x4 _presentViewProjection =
        Matrix4x4.CreateOrthographicOffCenterLeftHanded(0, 1, 1, 0, 0, 1);

    private static readonly Vector4 _fullTextureUvs = new(0, 0, 1, 1);

    private readonly GpuVertexBuffer<PositionTextureVertex> _vertexBuffer;
    private readonly GraphicsPipeline _colorPipeline;
    private readonly GraphicsPipeline _tintedTexturePipeline;
    private readonly GraphicsPipeline _presentPipeline;
    private readonly Sampler _sampler;
    public Texture RetainedTexture => _retainedTexture;

    private readonly GpuDevice _gpuDevice;
    private readonly TextureFormat _colorTargetFormat;

    private Texture _retainedTexture;
    private Texture _depthBuffer;
    private Matrix4x4 _viewProjection;

    private int _maxDepthValue;

    public PencuilRenderer(
        GraphicsPipelineBuilder graphicsPipelineBuilder,
        GpuMemorySystem gpuMemorySystem,
        ShaderLoader shaderLoader,
        GpuDevice gpuDevice,
        WindowManager windowManager)
    {
        ReadOnlySpan<PositionTextureVertex> quad =
        [
            new(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0, 0)),
            new(new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1, 0)),
            new(new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0, 1)),
            new(new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1, 1)),
        ];

        _vertexBuffer = gpuMemorySystem.CreateVertexBuffer(quad);

        VertexShader vertexShader = shaderLoader.LoadVertexShader("shaders/pencuil_vertex");
        FragmentShader colorFragmentShader = shaderLoader.LoadFragmentShader("shaders/pencuil_color_fragment");
        FragmentShader tintedTextureFragmentShader = shaderLoader.LoadFragmentShader("shaders/pencuil_tinted_texture_fragment");
        FragmentShader textureFragmentShader = shaderLoader.LoadFragmentShader("shaders/pencuil_texture_fragment");

        TextureFormat colorTargetFormat = windowManager.PrimaryWindow.ColorTargetFormat;
        ShortSize renderSize = windowManager.PrimaryWindow.RenderSizeInPixels;

        _colorPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfigBasedOnBuffer(_vertexBuffer)
            .SetShaders(vertexShader, colorFragmentShader)
            .AddColorTarget(colorTargetFormat, BlendingState.Standard)
            .EnableDepthTesting(DepthBufferFormat.Depth32)
            .SetCullMode(CullMode.None)
            .Build();

        _tintedTexturePipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfigBasedOnBuffer(_vertexBuffer)
            .SetShaders(vertexShader, tintedTextureFragmentShader)
            .AddColorTarget(colorTargetFormat, BlendingState.PremultipliedAlpha)
            .EnableDepthTesting(DepthBufferFormat.Depth32)
            .SetCullMode(CullMode.None)
            .Build();

        _presentPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfigBasedOnBuffer(_vertexBuffer)
            .SetShaders(vertexShader, textureFragmentShader)
            .AddColorTarget(colorTargetFormat, BlendingState.Standard)
            .Build();

        _gpuDevice = gpuDevice;
        _colorTargetFormat = colorTargetFormat;

        _sampler = gpuDevice.CreateSampler(SamplerConfig.PixelArt);
        _retainedTexture = gpuDevice.CreateColorTargetTexture(renderSize, colorTargetFormat);
        _depthBuffer = gpuDevice.CreateDepthBufferTexture(renderSize, DepthBufferFormat.Depth32);

        _viewProjection = Matrix4x4.CreateOrthographicOffCenterLeftHanded(0, renderSize.Width, renderSize.Height, 0, 0, 1);
    }

    public void Resize(ShortSize newSize)
    {
        _retainedTexture.Dispose();
        _depthBuffer.Dispose();

        _retainedTexture = _gpuDevice.CreateColorTargetTexture(newSize, _colorTargetFormat);
        _depthBuffer = _gpuDevice.CreateDepthBufferTexture(newSize, DepthBufferFormat.Depth32);
        _viewProjection = Matrix4x4.CreateOrthographicOffCenterLeftHanded(0, newSize.Width, newSize.Height, 0, 0, 1);
    }

    public void Render(CommandBuffer commandBuffer, Pencil pencil)
    {
        if (pencil._coloredRectangleInstructions.Count == 0 && pencil._textureRegionInstructions.Count == 0)
        {
            using IRenderPass clearPass = new RenderPassBuilder(commandBuffer)
                .AddColorTarget(_retainedTexture, _guiColorTargetSettings)
                .Build();

            return;
        }

        _maxDepthValue = pencil._coloredRectangleInstructions.Count + pencil._textureRegionInstructions.Count;

        using IRenderPass renderPass = new RenderPassBuilder(commandBuffer)
            .AddColorTarget(_retainedTexture, _guiColorTargetSettings)
            .SetDepthBuffer(_depthBuffer, DepthBufferSettings.Default)
            .Build();

        commandBuffer.PushVertexUniformData(0, _viewProjection);

        renderPass.BindGraphicsPipeline(_colorPipeline);
        renderPass.BindVertexBuffer(_vertexBuffer);

        foreach (var instruction in pencil._coloredRectangleInstructions)
        {
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(
                instruction.Area.Width,
                instruction.Area.Height,
                1.0f);

            Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(
                instruction.Area.X,
                instruction.Area.Y,
                CalculateZCoordinate(instruction.Depth));

            Matrix4x4 worldMatrix = scaleMatrix * translationMatrix;

            commandBuffer.PushVertexUniformData(1, worldMatrix);
            commandBuffer.PushFragmentUniformData(0, (FColor)instruction.Color);

            renderPass.DrawPrimitive();
        }

        if (pencil._textureRegionInstructions.Count > 0)
        {
            renderPass.BindGraphicsPipeline(_tintedTexturePipeline);
            renderPass.BindVertexBuffer(_vertexBuffer);

            foreach (var instruction in pencil._textureRegionInstructions)
            {
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(
                    instruction.Area.Width,
                    instruction.Area.Height,
                    1.0f);

                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(
                    instruction.Area.X,
                    instruction.Area.Y,
                    CalculateZCoordinate(instruction.Depth));

                Matrix4x4 worldMatrix = scaleMatrix * translationMatrix;

                commandBuffer.PushVertexUniformData(1, worldMatrix);
                commandBuffer.PushFragmentUniformData(0, instruction.Uvs);
                commandBuffer.PushFragmentUniformData(1, instruction.Tint);

                renderPass.BindFragmentSampler(instruction.Texture, _sampler);
                renderPass.DrawPrimitive();
            }
        }

    }

    public void Present(CommandBuffer commandBuffer, Texture target, bool clearTarget)
    {
        var settings = clearTarget
            ? ColorTargetSettings.Clear
            : new ColorTargetSettings { LoadOperation = LoadOperation.Load };

        using IRenderPass presentPass = new RenderPassBuilder(commandBuffer)
            .AddColorTarget(target, settings)
            .Build();

        commandBuffer.PushVertexUniformData(0, _presentViewProjection);
        commandBuffer.PushVertexUniformData(1, Matrix4x4.Identity);
        commandBuffer.PushFragmentUniformData(0, _fullTextureUvs);

        presentPass.BindGraphicsPipeline(_presentPipeline);
        presentPass.BindVertexBuffer(_vertexBuffer);
        presentPass.BindFragmentSampler(_retainedTexture, _sampler);
        presentPass.DrawPrimitive();
    }

    private float CalculateZCoordinate(int elementDepth)
    {
        return (_maxDepthValue - elementDepth) / (float)(_maxDepthValue + 1);
    }
}
