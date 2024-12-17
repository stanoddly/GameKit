using GameKit;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.ImageLoader.StbImageSharp;
using GameKit.Modules;

namespace Samples.TexturedQuad;

public class MyGame(
    GpuDevice gpuDevice,
    GraphicsPipelineBuilder graphicsPipelineBuilder,
    IContentLoader<Shader> shaderLoader,
    IContentLoader<Image> imageLoader)
    : IInitializable, IDrawable
{
    private GraphicsPipeline _graphicsPipeline;
    private VertexBuffer<PositionTextureVertex> _vertexBuffer;
    private Texture _texture = null!;
    private Sampler _sampler;

    public void Initialize()
    {
        Shader vertexShader = shaderLoader.Load("TexturedQuadVertex.spak.json");
        Shader fragmentShader  = shaderLoader.Load("TexturedQuadFragment.spak.json");

        _graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddColorFormatFromDisplay()
            .AddVertexBufferConfig<PositionTextureVertex>()
            .SetShaders(vertexShader, fragmentShader)
            .Build();

        using (GpuMemoryTransfer memoryTransfer = gpuDevice.CreateMemoryTransfer())
        {
            _vertexBuffer = memoryTransfer.AddVertexBuffer(PositionTextureShapes.HorizontalQuad);
    
            using Image image = imageLoader.Load("Earth.png");
            _texture = memoryTransfer.AddTexture(image);
        }

        _sampler = gpuDevice.CreatePixelArtSampler();
    }

    public void Draw()
    {
        using FrameRenderContext frameRenderContext = gpuDevice.CreateFrameRenderContext();

        using FrameRenderPass frameRenderPass = frameRenderContext.CreateRenderPass(_graphicsPipeline);

        frameRenderPass.BindVertexBuffer(_vertexBuffer);
        frameRenderPass.BindFragmentSampler(_texture, _sampler);
        frameRenderPass.DrawPrimitive();
    }

    public static int Main()
    {
        var gameKitApp = new GameKitApp()
            .AddContentFromProjectDirectory("Content")
            .AddStbImageLoader()
            .Add(new WindowConfiguration{ Size = (512, 512)})
            .AddScoped<MyGame>();

        return gameKitApp.Run();
    }
}
