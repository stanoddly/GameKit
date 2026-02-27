# Subrenderers

Subrenderers are renderers that receive an existing RenderPass instead of creating their own. They're used for internal composition within an `IRenderPhase<T>`.

```
DefaultRenderManager<T>
└─ IRenderPhase<T>[] (geometry, lighting, post-process phases)
    └─ Subrenderers (multiple renderers sharing the same RenderPass)
```

The parent phase creates the RenderPass and multiple subrenderers contribute to the same render targets.

## Basic Pattern

A subrenderer receives both a `CommandBuffer` and an `IRenderPass`:

```csharp
public class MeshSubrenderer
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly MeshBufferService _bufferService;
    private readonly Camera _camera;

    public MeshSubrenderer(GraphicsPipeline graphicsPipeline, MeshBufferService bufferService, Camera camera)
    {
        _graphicsPipeline = graphicsPipeline;
        _bufferService = bufferService;
        _camera = camera;
    }

    public void Render(CommandBuffer commandBuffer, IRenderPass renderPass)
    {
        // Don't create a new RenderPass - use the one provided

        Matrix4x4 viewProjection = _camera.ViewMatrix * _camera.ProjectionMatrix;
        commandBuffer.PushVertexUniformData(0, viewProjection);

        renderPass.BindGraphicsPipeline(_graphicsPipeline);

        foreach (var entry in _bufferService.RenderableEntries)
        {
            renderPass.BindVertexBuffer(entry.Buffer);
            commandBuffer.PushVertexUniformData(1, entry.WorldMatrix);
            renderPass.DrawPrimitive();
        }
    }

    public static MeshSubrenderer Create(/* dependencies */)
    {
        // Build pipeline, return instance
    }
}
```

## Composing Multiple Subrenderers

### Defining a Subrenderer Interface

Create an interface for your specific rendering phase:

```csharp
public interface IGeometrySubrenderer : IOrderable
{
    void Render(CommandBuffer commandBuffer, IRenderPass renderPass);
}
```

Inherit from `IOrderable` (from GameKit) to control execution order.

### Implementing the Interface

```csharp
public class MeshSubrenderer : IGeometrySubrenderer
{
    private readonly GraphicsPipeline _graphicsPipeline;
    private readonly MeshBufferService _bufferService;
    private readonly Camera _camera;

    public int Order => 100; // From IOrderable

    public MeshSubrenderer(GraphicsPipeline graphicsPipeline, MeshBufferService bufferService, Camera camera)
    {
        _graphicsPipeline = graphicsPipeline;
        _bufferService = bufferService;
        _camera = camera;
    }

    public void Render(CommandBuffer commandBuffer, IRenderPass renderPass)
    {
        // Implementation
    }
}
```

### Parent Phase Orchestration

The parent phase (implementing `IRenderPhase<T>`) injects all subrenderers and orders them:

```csharp
public class GeometryPhase : IRenderPhase<GameRenderContext>
{
    private readonly IReadOnlyList<IGeometrySubrenderer> _subrenderers;
    private readonly GameRenderContextBuffers _buffers;

    public GeometryPhase(
        IEnumerable<IGeometrySubrenderer> subrenderers,
        GameRenderContextBuffers buffers)
    {
        _subrenderers = subrenderers.OrderBy(r => r.Order).ToList();
        _buffers = buffers;
    }

    public void Render(GameRenderContext renderContext)
    {
        using IRenderPass renderPass = new RenderPassBuilder(renderContext.CommandBuffer)
            .AddColorTarget(_buffers.AlbedoBuffer.Texture)
            .AddColorTarget(_buffers.NormalBuffer.Texture)
            .AddColorTarget(_buffers.PositionBuffer.Texture)
            .SetSharedColorTargetSettings(ColorTargetSettings.Clear)
            .Build();

        foreach (IGeometrySubrenderer subrenderer in _subrenderers)
        {
            subrenderer.Render(renderContext.CommandBuffer, renderPass);
        }

        // RenderPass disposed here - all subrenderers have contributed
    }
}
```

**Note:** `IRenderPhase<T>` is managed by `DefaultRenderManager<T>`, which orchestrates multiple phases (geometry, lighting, post-process) in order.

## Key Points

- **Don't create RenderPass**: Subrenderers receive an existing RenderPass from the parent
- **IOrderable**: Use this interface to control execution order (lower numbers execute first)
- **IEnumerable injection**: Parent renderer can accept `IEnumerable<IYourSubrenderer>` and order them in the constructor
- **Shared render targets**: All subrenderers draw into the same outputs within a single RenderPass
- **One RenderPass disposal**: The parent manages RenderPass lifetime, not the subrenderers

## When to Use

- **Within a render phase**: When a single `IRenderPhase<T>` needs to compose multiple rendering operations
- **Shared render targets**: Multiple rendering operations need to write to the same G-buffer or output textures
- **Extensible phase implementation**: Allow easy addition of new rendering contributions without modifying the phase

Note: If you need separate RenderPasses, implement multiple `IRenderPhase<T>` classes instead.
