using GameKit;

using var gameKitApp = new GameKitAppBuilder()
    .WithRootDirectoryFromExecutable("Content")
    .Build();

Shader vertexShader = gameKitApp.ShaderBuilder.UseVertexStage().FromFileBasedOnFormat("PositionColor").Build();
Shader fragmentShader = gameKitApp.ShaderBuilder.UseFragmentStage().FromFileBasedOnFormat("SolidColor").Build();

RenderingPipeline renderingPipeline = gameKitApp.GraphicsPipelineBuilder
    .AddColorFormatFromDisplay()
    .AddVertexBufferConfig<PositionColorVertex>()
    .UseShaders(vertexShader, fragmentShader)
    .Build();

Span<PositionColorVertex> vertices =
[
    (-1, -1, 0, 255, 0, 0, 255),
    (1, -1, 0, 0, 255, 0, 255),
    (0, 1, 0, 0, 0, 255, 255)
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