using GameKit;
using GameKit.ImageLoader.StbImageSharp;

using var gameKitApp = new GameKitAppBuilder()
    .AddContentFromProjectDirectory("Content")
    .UseStbImageLoader()
    .WithSize(512, 512)
    .Build();

Shader vertexShader = gameKitApp.LoadShader("TexturedQuadVertex.spak.json");
Shader fragmentShader  = gameKitApp.LoadShader("TexturedQuadFragment.spak.json");

GraphicsPipeline graphicsPipeline = gameKitApp.GraphicsPipelineBuilder
    .SetPrimitiveType(PrimitiveType.TriangleStrip)
    .AddColorFormatFromDisplay()
    .AddVertexBufferConfig<PositionTextureVertex>()
    .SetShaders(vertexShader, fragmentShader)
    .Build();

VertexBuffer<PositionTextureVertex> vertexBuffer;
Texture texture;

using (GpuMemoryTransfer memoryTransfer = gameKitApp.GpuDevice.CreateMemoryTransfer())
{
    vertexBuffer = memoryTransfer.AddVertexBuffer(PositionTextureShapes.HorizontalQuad);
    
    using Image image = gameKitApp.ContentManager.Load<Image>("Earth.png");
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
