using GameKit;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Modules;

var gameKitApp = new GameKitApp()
    .AddContentFromProjectDirectory("Content")
    .AddScoped<MyService>();

return gameKitApp.Run();

public class MyService: IPreparable, IDrawable
{
    private readonly GpuDevice _gpuDevice;
    private readonly IContentLoader<Shader> _shaderLoader;
    private readonly GraphicsPipelineBuilder _pipelineBuilder;
    private GraphicsPipeline _graphicsPipeline;
    VertexBuffer<PositionColorVertex> _vertexBuffer;

    public MyService(GpuDevice gpuDevice, IContentLoader<Shader> shaderLoader, GraphicsPipelineBuilder pipelineBuilder)
    {
        _gpuDevice = gpuDevice;
        _shaderLoader = shaderLoader;
        _pipelineBuilder = pipelineBuilder;
    }

    public void Prepare()
    {
        Shader vertexShader = _shaderLoader.Load("PositionColor.spak.json");
        Shader fragmentShader  = _shaderLoader.Load("SolidColor.spak.json");

        _graphicsPipeline = _pipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddColorFormatFromDisplay()
            .AddVertexBufferConfig<PositionColorVertex>()
            .SetShaders(vertexShader, fragmentShader)
            .Build();

        ReadOnlySpan<PositionColorVertex> vertices =
        [
            (-1, -1, 0, 255, 0, 0, 255),
            (1, -1, 0, 0, 255, 0, 255),
            (0, 1, 0, 0, 0, 255, 255)
        ];
        
        using (GpuMemoryTransfer memoryTransfer = _gpuDevice.CreateMemoryTransfer())
        {
            _vertexBuffer = memoryTransfer.AddVertexBuffer(vertices);
        }
    }

    public void Draw()
    {
        using FrameRenderContext frameRenderContext = _gpuDevice.CreateFrameRenderContext();

        using FrameRenderPass frameRenderPass = frameRenderContext.CreateRenderPass(_graphicsPipeline);

        frameRenderPass.BindVertexBuffer(_vertexBuffer);
        frameRenderPass.DrawPrimitive();
    }
}