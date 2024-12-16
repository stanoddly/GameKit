using GameKit;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.ImageLoader.StbImageSharp;
using GameKit.Modules;

var gameKitApp = new GameKitApp()
    .AddContentFromProjectDirectory("Content")
    .AddStbImageLoader()
    .Add(new WindowConfiguration{ Size = (512, 512)})
    .AddScoped<MyService>();

return gameKitApp.Run();


public class MyService: IPreparable, IDrawable
{
    private readonly IContentLoader<Shader> _contentLoader;
    private readonly IContentLoader<Image> _imageLoader;
    private readonly GraphicsPipelineBuilder _graphicsPipelineBuilder;
    private readonly GpuDevice _gpuDevice;

    private GraphicsPipeline _graphicsPipeline;
    private VertexBuffer<PositionTextureVertex> _vertexBuffer;
    private Texture _texture;
    private Sampler _sampler;

    public MyService(IContentLoader<Shader> contentLoader, GraphicsPipelineBuilder graphicsPipelineBuilder, GpuDevice gpuDevice, IContentLoader<Image> imageLoader)
    {
        _contentLoader = contentLoader;
        _graphicsPipelineBuilder = graphicsPipelineBuilder;
        _gpuDevice = gpuDevice;
        _imageLoader = imageLoader;
    }


    public void Prepare()
    {
        Shader vertexShader = _contentLoader.Load("TexturedQuadVertex.spak.json");
        Shader fragmentShader  = _contentLoader.Load("TexturedQuadFragment.spak.json");

        _graphicsPipeline = _graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddColorFormatFromDisplay()
            .AddVertexBufferConfig<PositionTextureVertex>()
            .SetShaders(vertexShader, fragmentShader)
            .Build();

        using (GpuMemoryTransfer memoryTransfer = _gpuDevice.CreateMemoryTransfer())
        {
            _vertexBuffer = memoryTransfer.AddVertexBuffer(PositionTextureShapes.HorizontalQuad);
    
            using Image image = _imageLoader.Load("Earth.png");
            _texture = memoryTransfer.AddTexture(image);
        }

        _sampler = _gpuDevice.CreatePixelArtSampler();
    }

    public void Draw()
    {
        using FrameRenderContext frameRenderContext = _gpuDevice.CreateFrameRenderContext();

        using FrameRenderPass frameRenderPass = frameRenderContext.CreateRenderPass(_graphicsPipeline);

        frameRenderPass.BindVertexBuffer(_vertexBuffer);
        frameRenderPass.BindFragmentSampler(_texture, _sampler);
        frameRenderPass.DrawPrimitive();
    }
}