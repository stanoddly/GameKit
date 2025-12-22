using System.Numerics;
using GameKit.Common;
using GameKit.Gpu;
using GameKit.Shaders;
using GameKit.Sprites;
using GameKit.Text;
using Microsoft.Extensions.DependencyInjection;

namespace GameKit.Uiui;

public static class GuiRendererFactory
{
    public static GuiRenderer Create(IServiceProvider serviceProvider)
    {
        IGpuDevice gpuDevice = serviceProvider.GetRequiredService<IGpuDevice>();
        GraphicsPipelineBuilder graphicsPipelineBuilder = serviceProvider.GetRequiredService<GraphicsPipelineBuilder>();
        ShaderLoader shaderLoader = serviceProvider.GetRequiredService<ShaderLoader>();
        IGuiRendererConfig guiRendererConfig = serviceProvider.GetRequiredService<IGuiRendererConfig>();
        GuiResolutionProvider guiResolutionProvider = serviceProvider.GetRequiredService<GuiResolutionProvider>();
        GpuMemorySystem gpuMemorySystem = serviceProvider.GetRequiredService<GpuMemorySystem>();

        ReadOnlySpan<PositionTextureVertex> verticalQuad =
        [
            new(new Vector3(0.0f, 0.0f, 0.0f), new Vector2(0, 0)),
            new(new Vector3(1.0f, 0.0f, 0.0f), new Vector2(1, 0)),
            new(new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0, 1)),
            new(new Vector3(1.0f, 1.0f, 0.0f), new Vector2(1, 1)),
        ];

        GpuVertexBuffer<PositionTextureVertex> vertexBuffer = gpuMemorySystem.CreateVertexBuffer(verticalQuad);

        Shader vertexShader = shaderLoader.Load("shaders/basic_transform_vertex");
        Shader textureFragmentShader = shaderLoader.Load("shaders/texture_fragment");
        Shader colorFragmentShader = shaderLoader.Load("shaders/color_fragment");
        Shader textureColorFragmentShader = shaderLoader.Load("shaders/texture_color_fragment");

        var texturePipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfigBasedOnBuffer(vertexBuffer)
            .SetShaders(vertexShader, textureFragmentShader)
            .AddColorTarget(guiRendererConfig.ColorTargetFormat, BlendingState.Standard)
            .EnableDepthTesting(guiRendererConfig.DepthBufferFormat)
            .Build();

        var colorPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfigBasedOnBuffer(vertexBuffer)
            .SetShaders(vertexShader, colorFragmentShader)
            .AddColorTarget(guiRendererConfig.ColorTargetFormat, BlendingState.Standard)
            .EnableDepthTesting(guiRendererConfig.DepthBufferFormat)
            .Build();

        var textureColorPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfigBasedOnBuffer(vertexBuffer)
            .SetShaders(vertexShader, textureColorFragmentShader)
            .AddColorTarget(guiRendererConfig.ColorTargetFormat, BlendingState.Standard)
            .EnableDepthTesting(guiRendererConfig.DepthBufferFormat)
            .Build();

        var sampler = gpuDevice.CreateSampler(SamplerConfig.PixelArt);

        return new GuiRenderer(vertexBuffer, texturePipeline, colorPipeline, textureColorPipeline, sampler, guiResolutionProvider);
    }
}

public sealed class GuiRenderer
{
    private static ColorTargetSettings _guiColorTargetSettings = new()
    {
        ClearColorValue = FColors.Transparent
    };
    private readonly GpuVertexBuffer<PositionTextureVertex> _vertexBuffer;
    private readonly GraphicsPipeline _texturePipeline;
    private readonly GraphicsPipeline _colorPipeline;
    private readonly GraphicsPipeline _textureColorPipeline;
    private readonly Sampler _sampler;
    private readonly GuiResolutionProvider _guiResolutionProvider;

    private readonly List<FilledRectangleInfo> _filledRectangles = new();
    private readonly List<SpritePositionInfo> _sprites = new();
    private readonly List<ColoredSpritePositionInfo> _coloredSprites = new();
    
    private int _maxDepthValue;

    internal GuiRenderer(GpuVertexBuffer<PositionTextureVertex> vertexBuffer, 
        GraphicsPipeline texturePipeline, GraphicsPipeline colorPipeline, GraphicsPipeline textureColorPipeline, Sampler sampler, GuiResolutionProvider guiResolutionProvider)
    {
        _vertexBuffer = vertexBuffer;
        _texturePipeline = texturePipeline;
        _colorPipeline = colorPipeline;
        _textureColorPipeline = textureColorPipeline;
        _sampler = sampler;
        _guiResolutionProvider = guiResolutionProvider;
    }

    internal void Touch()
    {
        _maxDepthValue++;
    }

    public bool NeedsRender => _maxDepthValue != 0;

    public void DrawFilledRectangle(ShortRectangle rect, FColor color)
    {
        _filledRectangles.Add(new FilledRectangleInfo(rect, color, _maxDepthValue++));
    }

    public void DrawSprite(ShortRectangle destination, SpriteAsset sprite)
    {
        _sprites.Add(new SpritePositionInfo(destination, sprite.Texture, sprite.CalculateTextureRegionUVs(), _maxDepthValue++));
    }

    public void DrawText(ShortRectangle destination, TextSpriteAsset textSprite, FColor color)
    {
        _coloredSprites.Add(new ColoredSpritePositionInfo(destination, textSprite.Texture, textSprite.CalculateTextureRegionUVs(), color, _maxDepthValue++));
    }

    public void DrawText(ShortVector2 position, TextSpriteAsset textSprite, FColor color)
    {
        var destination = new ShortRectangle(position, textSprite.Size);
        DrawText(destination, textSprite, color);
    }

    private void RenderGui(CommandBuffer commandBuffer, IRenderPass renderPass)
    {
        renderPass.BindVertexBuffer(_vertexBuffer);

        ushort scale = _guiResolutionProvider.ResolutionInfo.ScaleFactor;

        if (_filledRectangles.Count > 0)
        {
            renderPass.BindGraphicsPipeline(_colorPipeline);

            FColor color = default;
            foreach (var rect in _filledRectangles)
            {
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(
                    rect.Rectangle.Width * scale, 
                    rect.Rectangle.Height * scale, 
                    1.0f);
                
                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(
                    rect.Rectangle.X * scale,
                    rect.Rectangle.Y * scale,
                    CalculateZCoordinate(rect.Depth));
                
                Matrix4x4 worldMatrix = scaleMatrix * translationMatrix;

                if (color != rect.Color)
                {
                    commandBuffer.PushFragmentUniformData(0, rect.Color);
                }
                
                commandBuffer.PushVertexUniformData(1, worldMatrix);

                renderPass.DrawPrimitive();
            }
        }

        if (_sprites.Count > 0)
        {
            renderPass.BindGraphicsPipeline(_texturePipeline);
            
            foreach (var sprite in _sprites)
            {
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(
                    sprite.Destination.Width * scale,
                    sprite.Destination.Height * scale,
                    1.0f);
                
                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(
                    sprite.Destination.X * scale,
                    sprite.Destination.Y * scale,
                    CalculateZCoordinate(sprite.Depth));
                
                Matrix4x4 worldMatrix = scaleMatrix * translationMatrix;
                
                commandBuffer.PushFragmentUniformData(0, sprite.Uvs);
                commandBuffer.PushVertexUniformData(1, worldMatrix);
                renderPass.BindFragmentSampler(sprite.Texture, _sampler);
                
                renderPass.DrawPrimitive();
            }
        }

        if (_coloredSprites.Count > 0)
        {
            renderPass.BindGraphicsPipeline(_textureColorPipeline);
            
            foreach (var sprite in _coloredSprites)
            {
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(
                    sprite.Destination.Width * scale,
                    sprite.Destination.Height * scale,
                    1.0f);
                
                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(
                    sprite.Destination.X * scale,
                    sprite.Destination.Y * scale,
                    CalculateZCoordinate(sprite.Depth));
                
                Matrix4x4 worldMatrix = scaleMatrix * translationMatrix;
                
                commandBuffer.PushFragmentUniformData(0, sprite.Uvs);
                commandBuffer.PushFragmentUniformData(1, sprite.Color);
                commandBuffer.PushVertexUniformData(1, worldMatrix);
                renderPass.BindFragmentSampler(sprite.Texture, _sampler);
                
                renderPass.DrawPrimitive();
            }
        }

        _filledRectangles.Clear();
        _sprites.Clear();
        _coloredSprites.Clear();
        _maxDepthValue = 0;
    }

    public void Render(CommandBuffer commandBuffer, Texture guiTexture, Texture guiDepthBuffer, Matrix4x4 guiViewMatrix, Matrix4x4 guiProjectionMatrix)
    {
        if (!NeedsRender)
        {
            return;
        }

        using IRenderPass guiRenderPass = new RenderPassBuilder(commandBuffer)
            .AddColorTarget(guiTexture, _guiColorTargetSettings)
            .SetDepthBuffer(guiDepthBuffer, DepthBufferSettings.Default)
            .Build();

        Matrix4x4 viewProjection = guiViewMatrix * guiProjectionMatrix;
        commandBuffer.PushVertexUniformData(0, viewProjection);

        RenderGui(commandBuffer, guiRenderPass);
    }

    private float CalculateZCoordinate(int elementDepth)
    {
        // Maps render order to Z coordinates in [-1.0, 0.0] range for UI layering.
        // Earlier elements (lower depth values) get Z values closer to 0.0 (front)
        // Later elements (higher depth values) get Z values closer to -1.0 (back)
        // This ensures proper UI element layering without Z-fighting.
        return (elementDepth - _maxDepthValue) / (float)_maxDepthValue;
    }

    private readonly record struct FilledRectangleInfo(ShortRectangle Rectangle, FColor Color, int Depth);
    private readonly record struct SpritePositionInfo(ShortRectangle Destination, Texture Texture, Vector4 Uvs, int Depth);
    private readonly record struct ColoredSpritePositionInfo(ShortRectangle Destination, Texture Texture, Vector4 Uvs, FColor Color, int Depth);
}
