using GameKit;
using GameKit.ImageLoader.StbImageSharp;

using var gameKitApp = new GameKitAppBuilder()
    .UseStbImageLoader()
    .WithSize(512, 512)
    .Build();

Shader vertexShader = gameKitApp.LoadShader("Content/TexturedQuadVertex.spak.json");
Shader fragmentShader  = gameKitApp.LoadShader("Content/TexturedQuadFragment.spak.json");

GraphicsPipeline graphicsPipeline = gameKitApp.GraphicsPipelineBuilder
    .SetPrimitiveType(PrimitiveType.TriangleStrip)
    .AddColorFormatFromDisplay()
    .AddVertexBufferConfig<PositionTextureVertex>()
    .SetShaders(vertexShader, fragmentShader)
    .Build();

Span<PositionTextureVertex> vertices =
[
    (-1,  1, 0, 0, 0),  // Top-left
    (1,  1, 0, 1, 0),   // Top-right
    (-1, -1, 0, 0, 1),  // Bottom-left
    (1, -1, 0, 1, 1)    // Bottom-right
];

VertexBuffer<PositionTextureVertex> vertexBuffer;
Texture texture;

using (MemoryTransfer memoryTransfer = gameKitApp.GpuMemoryUploader.CreateMemoryTransfer())
{
    vertexBuffer = memoryTransfer.AddVertexBuffer(vertices);
    
    using Image image = gameKitApp.ContentManager.Load<Image>("Content/Earth.png");
    texture = memoryTransfer.AddTexture(image);
}

Sampler sampler = gameKitApp.GpuDevice.CreatePixelArtSampler();

gameKitApp.Draw(app =>
{
    using FrameRenderContext frameRenderContext = app.GpuDevice.CreateFrameRenderContext();

    using FrameRenderPass frameRenderPass = frameRenderContext.CreateRenderPass(graphicsPipeline);

    frameRenderPass.BindVertexBuffer(vertexBuffer);
    frameRenderPass.BindFragmentSampler(texture, sampler);
    frameRenderPass.DrawPrimitive();
});

return gameKitApp.Run();
