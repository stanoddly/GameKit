using GameKit.Gpu;
using GameKit.Shaders;

namespace GameKit.Tutorials.MultiWindow;

public class SecondaryWindowRenderer : IUpdatable, IDisposable
{
    private readonly WindowManager _windowManager;
    private readonly GpuDevice _gpuDevice;
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly GpuVertexBuffer<PositionVertex> _vertexBuffer;
    private Window? _secondaryWindow;

    public SecondaryWindowRenderer(
        WindowManager windowManager,
        GpuDevice gpuDevice,
        GraphicsPipeline graphicsPipeline,
        GpuVertexBuffer<PositionVertex> vertexBuffer,
        Window secondaryWindow)
    {
        _windowManager = windowManager;
        _gpuDevice = gpuDevice;
        _graphicsPipeline = graphicsPipeline;
        _vertexBuffer = vertexBuffer;
        _secondaryWindow = secondaryWindow;
    }

    public void Update()
    {
        if (_secondaryWindow == null)
        {
            return;
        }

        if (!_windowManager.Windows.Contains(_secondaryWindow))
        {
            _secondaryWindow = null;
            return;
        }

        CommandBuffer commandBuffer = _gpuDevice.AcquireCommandBuffer();
        if (!_secondaryWindow.TryWaitAndAcquireSwapchainTexture(commandBuffer, out SwapchainTexture swapchainTexture))
        {
            commandBuffer.Dispose();
            return;
        }

        commandBuffer.PushFragmentUniformData(0, FColors.Coral);
        using (IRenderPass renderPass = new RenderPassBuilder(commandBuffer)
                   .AddColorTarget(swapchainTexture)
                   .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
                   .Build())
        {
            renderPass.BindGraphicsPipeline(_graphicsPipeline);
            renderPass.BindVertexBuffer(_vertexBuffer);
            renderPass.DrawPrimitive();
        }

        commandBuffer.Submit();
    }

    public void Dispose()
    {
        if (_secondaryWindow != null && _windowManager.Windows.Contains(_secondaryWindow))
        {
            _windowManager.DestroyWindow(_secondaryWindow);
        }

        _secondaryWindow = null;
    }

    public static SecondaryWindowRenderer Create(
        WindowManager windowManager,
        GpuDevice gpuDevice,
        ShaderLoader shaderLoader,
        GraphicsPipelineBuilder graphicsPipelineBuilder,
        GpuMemorySystem gpuMemorySystem)
    {
        Window secondaryWindow = windowManager.CreateWindow(new WindowOptions(
            Size: new Size<uint>(480, 360),
            Title: "Secondary Window"));

        GpuVertexBuffer<PositionVertex> vertexBuffer = gpuMemorySystem.CreateVertexBuffer(PositionShapes.VerticalQuad);

        GraphicsPipeline graphicsPipeline = graphicsPipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaderProgram("shaders/shader")
            .AddColorFormatFromDisplay(secondaryWindow)
            .Build();

        return new SecondaryWindowRenderer(windowManager, gpuDevice, graphicsPipeline, vertexBuffer, secondaryWindow);
    }
}
