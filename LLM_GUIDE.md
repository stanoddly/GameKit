# GameKit LLM Reference

.NET 10+ game framework using SDL3 GPU API. C# 14, nullable enabled, unsafe allowed.

## Project Structure

```
src/
├── GameKit/                  # Core engine (entry point, GPU, content, input)
├── GameKit.Collections/      # High-perf data structures (Jinja2 generated)
├── GameKit.Common/           # Shared types (IntVector2, Size, IInitializable)
├── GameKit.Componentize/     # ECS: GameObject, GameComponent, GameWorld
├── GameKit.Encs/             # EventBus system
├── GameKit.RenderOrchestration/  # Multi-stage render pipeline
├── GameKit.SdlangCompileLib/     # Slang shader compiler integration
├── GameKit.SdlangCompileTask/    # MSBuild shader compilation task
├── GameKit.AStar/            # A* pathfinding
├── GameKit.Utils/            # Camera utilities
├── GameKit.Uiui/             # UI components
├── GameKit.ImageLoader.StbImageSharp/
└── GameKit.VertexShaderOnly/ # Vertex-only pipeline config
tests/                        # NUnit tests
tutorials/                    # Example projects
```

## Key Dependencies

- `ppy.SDL3-CS` - SDL3 bindings
- `Microsoft.Extensions.DependencyInjection` - IoC
- `MessagePack` - Serialization
- Slang v2025.21 - Shader compiler (auto-downloaded)

## App Bootstrap Pattern

```csharp
var builder = new GameKitAppBuilder()
    .AddContentFromProjectDirectory("Content")
    .UseDefaultRenderManager();

builder.RegisterInstance(new AppConfig { Size = (1280, 720), Title = "Game" });
builder.RegisterFunc<MyRenderer>(MyRenderer.Create).As<IRenderPhase<DefaultRenderContext>>();

using IGameKitApp app = builder.Build();
return app.Run();
```

Key file: `src/GameKit/App/GameKitAppBuilder.cs`

## DI Registration

```csharp
// Instance
builder.RegisterInstance(config);

// Type (auto-resolved)
builder.RegisterType<MyService>().As<IMyService>();

// Factory function (dependencies injected)
builder.RegisterFunc<MyService>(sp => new MyService(sp.GetRequiredService<IDep>()));
```

## GPU Rendering Pattern

```csharp
public class MyRenderer : IRenderPhase<DefaultRenderContext>
{
    private readonly GraphicsPipeline _pipeline;
    private readonly GpuVertexBuffer<PositionVertex> _vertexBuffer;

    public void Render(DefaultRenderContext ctx)
    {
        ctx.CommandBuffer.PushFragmentUniformData(0, color);

        using IRenderPass pass = new RenderPassBuilder(ctx.CommandBuffer)
            .AddColorTarget(ctx.SwapchainTexture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();

        pass.BindGraphicsPipeline(_pipeline);
        pass.BindVertexBuffer(_vertexBuffer);
        pass.DrawPrimitive();
    }

    public static MyRenderer Create(
        ShaderLoader shaderLoader,
        GraphicsPipelineBuilder pipelineBuilder,
        GpuMemorySystem gpuMemory)
    {
        var vb = gpuMemory.CreateVertexBuffer(vertices);
        var pipeline = pipelineBuilder
            .SetPrimitiveType(PrimitiveType.TriangleStrip)
            .AddVertexBufferConfig<PositionVertex>()
            .SetShaders("shaders/vertex", "shaders/fragment")
            .AddColorFormatFromDisplay()
            .Build();
        return new MyRenderer(pipeline, vb);
    }
}
```

Key files:
- `src/GameKit/Gpu/GraphicsPipelineBuilder.cs`
- `src/GameKit/Gpu/RenderPassBuilder.cs`
- `src/GameKit/RenderOrchestration/IRenderPhase.cs`

## Vertex Types

Implement `IVertexType` with `VertexElements` static property:

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct PositionVertex(Vector3 Position) : IVertexType
{
    public static ImmutableArray<VertexElementFormat> VertexElements { get; } =
        [VertexElementFormat.Float3];
}
```

Built-in: `PositionVertex`, `PositionColorVertex`, `PositionTextureVertex`, `PositionNormalColorVertex`, `PositionTextureNormalVertex`

Key file: `src/GameKit/Gpu/VertexTypes.cs`

## Shaders (Slang)

Location: `Content/shaders/*.slang`
Compiled at build time via MSBuild task.

Vertex shader:
```hlsl
struct Input { float4 Position : TEXCOORD0; };
struct Output { float4 Position : SV_Position; };

[shader("vertex")]
Output main(Input input) {
    Output output;
    output.Position = input.Position;
    return output;
}
```

Fragment shader:
```hlsl
ConstantBuffer<float4> color: register(b0, space3);

[shader("fragment")]
float4 main() : SV_Target0 {
    return color;
}
```

Uniform data binding:
- `space3` = fragment uniforms (`PushFragmentUniformData`)
- `space2` = vertex uniforms (`PushVertexUniformData`)

## Core Interfaces

| Interface | Location | Purpose |
|-----------|----------|---------|
| `IGameKitApp` | App/ | Main app lifecycle |
| `IGpuDevice` | Gpu/ | GPU resource management |
| `IRenderPass` | Gpu/ | Render pass execution |
| `IRenderPhase<T>` | RenderOrchestration/ | Render stage |
| `IVertexType` | Gpu/ | Vertex buffer layout |
| `IInitializable` | Common/ | Post-DI initialization |
| `IUpdatable` | Common/ | Frame update callback |
| `VirtualFileSystem` | Content/ | File abstraction |

## Content System

```csharp
// Project directory (dev)
builder.AddContentFromProjectDirectory("Content");

// Zip archive (release)
builder.AddContentFromZipPattern("data*.pak");

// Load content
var shader = shaderLoader.Load("shaders/vertex");
var texture = textureLoader.Load("textures/sprite.png");
```

Key files: `src/GameKit/Content/`

## Lifecycle Hooks

Classes auto-registered to events if they implement:
- `IInitializable` - Called after DI resolution
- `IUpdatable` - Called each frame
- `IDisposable` - Called on shutdown

## Collections (Generated)

High-perf generic collections in `GameKit.Collections`:
- `DenseSlotMap<T>` - Slot-based storage with stable handles
- `SparseSet<T>` - Sparse set
- `MultiArray<T>` - Multi-dimensional arrays
- `FastList<T>` - Optimized list

Generated from Jinja2 templates in `scripts/`.

## ECS (GameKit.Componentize)

```csharp
public class Player : GameComponent
{
    public override void Update() { /* per-frame logic */ }
}

var world = new GameWorld();
var obj = world.CreateGameObject();
obj.AddComponent<Player>();
```

Key files: `src/GameKit.Componentize/`

## Event Bus

```csharp
// Subscribe (auto via OnActivated)
public class Handler { public void Handle(MyEvent e) { } }

// Publish
eventBus.Publish(new MyEvent());
```

Key file: `src/GameKit.Encs/EventBus.cs`

## Build Commands

```bash
dotnet build
dotnet test
dotnet run --project tutorials/GameKit.Tutorials.Triangle
```

## File Patterns

| Pattern | Location |
|---------|----------|
| Interfaces | `I*.cs` in relevant namespace |
| Builders | `*Builder.cs` |
| SDL interop | Internal classes wrapping SDL3 |
| Generated code | `*.cs` from `*.cs.jinja` |

## Common Edits

**Add render phase**: Implement `IRenderPhase<DefaultRenderContext>`, register in builder
**Add vertex type**: Implement `IVertexType` with `VertexElements`
**Add shader**: Create `.slang` in `Content/shaders/`, reference by path without extension
**Add service**: Register via `RegisterType<T>()` or `RegisterFunc<T>()`
**Add component**: Extend `GameComponent`, add to `GameObject`

## Namespace Map

- `GameKit.App` - Application bootstrap
- `GameKit.Gpu` - GPU device, pipelines, buffers, textures
- `GameKit.Content` - Virtual file system, loaders
- `GameKit.Shaders` - Shader loading, metadata
- `GameKit.Input` - Keyboard, mouse, gamepad
- `GameKit.Text` - Font/text rendering
- `GameKit.Common` - Shared primitives
- `GameKit.RenderOrchestration` - Render phase management
- `GameKit.Componentize` - ECS framework
- `GameKit.Encs` - Event bus
