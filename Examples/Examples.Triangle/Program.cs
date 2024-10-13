using GameKit;

using var gameKitApp = new GameKitAppBuilder()
    .Build();

Shader vertexShader = gameKitApp.LoadShader("Content/PositionColor.spak.json");
Shader fragmentShader  = gameKitApp.LoadShader("Content/SolidColor.spak.json");

RenderingPipeline renderingPipeline = gameKitApp.GraphicsPipelineBuilder
    .UsePrimitiveType(PrimitiveType.TriangleStrip)
    .AddColorFormatFromDisplay()
    .AddVertexBufferConfig<PositionColorVertex>()
    .UseShaders(vertexShader, fragmentShader)
    .Build();

Span<PositionColorVertex> vertices =
[
    (-1,  1, 0, 255, 0, 0, 255),
    (1,  1, 0, 0, 255, 0, 255),
    (-1, -1, 0, 255, 0, 255, 255),
    (1, -1, 0, 0, 0, 255, 255)
];

VertexBuffer<PositionColorVertex> vertexBuffer;

using (MemoryTransfer memoryTransfer = gameKitApp.GpuMemoryUploader.CreateMemoryTransfer())
{
    vertexBuffer = memoryTransfer.AddVertexBuffer(vertices);
}

gameKitApp.Draw(app =>
{
    using FrameRenderContext frameRenderContext = app.GpuDevice.CreateFrameRenderContext();

    using FrameRenderPass frameRenderPass = frameRenderContext.CreateRenderPass(renderingPipeline);

    frameRenderPass.BindVertexBuffer(vertexBuffer);
    frameRenderPass.DrawPrimitive();
});

return gameKitApp.Run();
