---
name: rendering
description: GameKit rendering pipeline, render phases, pipeline configuration, shaders, render pass flow, subrenderers, and push constants reference.
user-invocable: false
---

## Render Architecture

`DefaultRenderManager<TRenderContext>` orchestrates `IRenderPhase<TRenderContext>` instances sorted by `IOrderable.Order` (lower runs first). Each phase implements one rendering step (shadow map, geometry, lighting, post-process).

Register phases via DI:
```csharp
builder.RegisterFunc<ShadowMapPhase>(ShadowMapPhase.Create).As<IRenderPhase<GameRenderContext>>();
builder.RegisterType<GeometryPhase>().As<IRenderPhase<GameRenderContext>>();
```

### Custom Render Context

Extend `DefaultRenderContext` with project-specific resources:
```csharp
public class GameRenderContext : DefaultRenderContext
{
    public Texture DepthTexture { get; }
    public Texture SceneColor { get; }
    public IsometricCamera Camera { get; }
}
```

Provide via `IRenderContextProvider<GameRenderContext>` registered with `.As<>()`.

## Two Key Objects

### CommandBuffer
Records GPU commands. Lives for the entire frame. Used for:
- Pushing uniform data (push constants)
- Creating RenderPasses

### RenderPass
Active rendering context created from CommandBuffer. Used for:
- Binding pipelines, vertex buffers
- Drawing primitives
- **Disposed to execute** — rendering happens on dispose

## Render Pass Patterns

### Pattern 1: Create Own RenderPass (Phase Renderers)

```csharp
public void Render(GameRenderContext renderContext)
{
    // Push uniforms BEFORE pass
    renderContext.CommandBuffer.PushFragmentUniformData(0, color);

    // Create RenderPass
    using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
        .AddColorTarget(renderContext.SwapchainTexture)
        .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
        .Build();

    // Bind and draw
    renderPass.BindGraphicsPipeline(_graphicsPipeline);
    renderPass.BindVertexBuffer(_vertexBuffer);
    renderPass.DrawPrimitive();

    // RenderPass disposed here — commands execute
}
```

### Pattern 2: Receive Existing RenderPass (Subrenderers)

```csharp
public void Render(CommandBuffer commandBuffer, IRenderPass renderPass)
{
    // RenderPass already exists — don't create or dispose

    commandBuffer.PushVertexUniformData(0, viewProjection);
    renderPass.BindGraphicsPipeline(_graphicsPipeline);

    foreach (var item in items)
    {
        renderPass.BindVertexBuffer(item.Buffer);
        commandBuffer.PushVertexUniformData(1, item.WorldMatrix);
        renderPass.DrawPrimitive();
    }
}
```

### Binding Order

Inside a RenderPass:
1. **BindGraphicsPipeline** — sets pipeline state
2. **BindVertexBuffer** — binds vertex data
3. **PushUniformData** — per-draw data (optional)
4. **DrawPrimitive** — issues draw call (**must be last**)

For multiple objects, rebind vertex buffers and push new uniforms between draws.

### RenderPassBuilder

```csharp
new RenderPassBuilder(commandBuffer)
    .AddColorTarget(texture)
    .SetSharedColorTargetSettings(ColorTargetSettings.Clear)  // or Load
    .Build()
```

- `Clear` — clear target before rendering
- `Load` — keep existing contents
- Add multiple color targets for deferred rendering (G-buffer)
- Add depth target with `.SetDepthStencilTarget(depthTexture, DepthStencilTargetSettings.ClearReversedZ)` for depth-tested passes

## Push Constants (Uniforms)

Small per-draw data sent to shaders:
```csharp
commandBuffer.PushVertexUniformData(0, viewProjectionMatrix);   // Slot 0
commandBuffer.PushVertexUniformData(1, worldMatrix);             // Slot 1
commandBuffer.PushFragmentUniformData(0, color);                 // Slot 0 (fragment)
```

Slot numbers (0-3) must match shader `register(b{slot}, space3)` bindings. Can be called before or during RenderPass.

## Pipeline Configuration (GraphicsPipelineBuilder)

### Vertex Configuration

```csharp
.AddVertexBufferConfig<PositionVertex>()
.AddVertexBufferConfig<PositionNormalColorVertex>()
```

Order matters — matches shader input semantic order and `BindVertexBuffer()` call order.

**Available vertex types:**
- `PositionVertex` — position only
- `PositionColorVertex` — position + color
- `PositionTextureVertex` — position + texture coordinates
- `PositionNormalColorVertex` — position + normal + color
- `PositionTextureNormalVertex` — position + texture coordinates + normal
- `PositionTextureNormalColorVertex` — position + texture coordinates + normal + color

### Primitive Types

```csharp
.SetPrimitiveType(PrimitiveType.TriangleList)   // Every 3 vertices = 1 triangle (most common)
.SetPrimitiveType(PrimitiveType.TriangleStrip)  // Good for quads with 4 vertices
```

### Shader Loading

```csharp
// Direct path (most common)
.SetShaders("shaders/vertex", "shaders/fragment")

// Separate objects (for reuse across pipelines)
VertexShader vertexShader = shaderLoader.LoadVertexShader("shaders/terrain_vertex");
FragmentShader fragmentShader = shaderLoader.LoadFragmentShader("shaders/terrain_fragment");
.SetShaders(vertexShader, fragmentShader)
```

Paths are relative to `Content/` directory, exclude `.slang` extension.

### Color Targets

```csharp
// Single target (forward rendering)
.AddColorFormatFromDisplay()

// Multiple targets (deferred rendering)
.AddColorTarget(buffers.AlbedoBuffer.Format)    // SV_Target0
.AddColorTarget(buffers.NormalBuffer.Format)    // SV_Target1
.AddColorTarget(buffers.PositionBuffer.Format)  // SV_Target2
```

Order must match fragment shader `SV_Target` outputs.

### Depth Testing

```csharp
.EnableDepthTesting(depthFormat, writeDepth: true, compareOp: CompareOperation.ReversedLess)
```

- `writeDepth: true` for geometry, `false` for transparent/overlay
- **Use `CompareOperation.ReversedLess` (Reverse-Z)** for better depth precision across all ranges

### Complete Example

```csharp
GraphicsPipeline pipeline = graphicsPipelineBuilder
    .SetPrimitiveType(PrimitiveType.TriangleList)
    .AddVertexBufferConfig<PositionNormalColorVertex>()
    .SetShaders("shaders/terrain_vertex", "shaders/terrain_fragment")
    .AddColorTarget(buffers.AlbedoBuffer.Format)
    .AddColorTarget(buffers.NormalBuffer.Format)
    .AddColorTarget(buffers.PositionBuffer.Format)
    .EnableDepthTesting(buffers.DepthBuffer.Format, true, CompareOperation.ReversedLess)
    .Build();
```

Pipeline is immutable after build. `GraphicsPipelineBuilder` is injected via DI. Store pipelines, don't rebuild every frame.

## Shaders (Slang)

Shaders are written in Slang and compiled to SPIR-V at build time.

### File Structure
```
Content/shaders/
├── vertex.slang              # Source files
├── fragment.slang
└── compiled/                 # Generated at build time
    ├── vertex.spv
    ├── vertex.metadata.json
    ├── fragment.spv
    └── fragment.metadata.json
```

### Vertex Shader

```csharp
struct Input
{
    float4 Position : TEXCOORD0;
    float4 Color : TEXCOORD1;
};

struct Output
{
    float4 Position : SV_Position;
    float4 Color : TEXCOORD0;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;
    output.Position = input.Position;
    output.Color = input.Color;
    return output;
}
```

**Input semantics:** `TEXCOORD0`, `TEXCOORD1`, etc. Order must match C# vertex struct fields.
**Output:** Always `SV_Position` for position. Interpolants use `TEXCOORD` semantics.

### Fragment Shader

```csharp
[shader("fragment")]
float4 main() : SV_Target0
{
    return float4(1.0, 0.0, 1.0, 1.0);
}
```

**Output:** `SV_Target0`, `SV_Target1`, etc. for MRT. Order matches `AddColorTarget()` calls.

### Constant Buffers

```csharp
ConstantBuffer<float4> color : register(b0, space3);

[shader("fragment")]
float4 main() : SV_Target0
{
    return color;
}
```

**Register binding:** `register(b{slot}, space3)` where slot is 0-3. `space3` is required by convention.

**C# side:**
```csharp
renderPass.PushFragmentUniformData(0, FColors.Magenta);  // Matches b0
```

### Multiple Render Targets (MRT)

```csharp
struct Output
{
    float4 Albedo : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Position : SV_Target2;
};

[shader("fragment")]
Output main()
{
    Output output;
    // fill output...
    return output;
}
```

### Shader Rules
- Entry point is always `main`
- Always mark with `[shader("vertex")]` or `[shader("fragment")]`
- Always use explicit `register` bindings for constant buffers
- SPIR-V format (cross-platform: Vulkan/Metal/D3D12)
- Compilation is cached by source hash, recompiles only on change

## Subrenderers

Subrenderers receive an existing RenderPass from a parent phase. Used for internal composition within an `IRenderPhase<T>`.

### Define a Subrenderer Interface

```csharp
public interface IGeometrySubrenderer : IOrderable
{
    void Render(CommandBuffer commandBuffer, IRenderPass renderPass);
}
```

Inherit from `IOrderable` to control execution order.

### Parent Phase Orchestration

```csharp
public class GeometryPhase : IRenderPhase<GameRenderContext>
{
    private readonly IReadOnlyList<IGeometrySubrenderer> _subrenderers;

    public GeometryPhase(IEnumerable<IGeometrySubrenderer> subrenderers, GameRenderContextBuffers buffers)
    {
        _subrenderers = subrenderers.OrderBy(r => r.Order).ToList();
        _buffers = buffers;
    }

    public void Render(GameRenderContext renderContext)
    {
        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(_buffers.AlbedoBuffer.Texture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();

        foreach (IGeometrySubrenderer subrenderer in _subrenderers)
        {
            subrenderer.Render(renderContext.CommandBuffer, renderPass);
        }
    }
}
```

### Key Rules
- **Phase renderers** create and dispose RenderPass
- **Subrenderers** receive RenderPass — never create or dispose it
- Use `IEnumerable<T>` injection + `OrderBy(r => r.Order)` for ordering
- If you need separate RenderPasses, use multiple `IRenderPhase<T>` classes instead

## Key Insights

- Push constants can be called before or during RenderPass
- RenderPass disposal triggers actual GPU work
- One RenderPass can have many draw calls
- Changing pipelines mid-pass is valid but expensive
- Phase ordering uses `IOrderable.Order` — use negative values for early passes (shadow maps), positive for later (post-process)
