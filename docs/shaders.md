# Shaders

Guide to writing and using shaders with GameKit. Shaders are written in Slang and compiled to SPIR-V at build time.

## File Structure

```
Content/shaders/
├── vertex.slang          # Source shader files
├── fragment.slang
└── compiled/             # Generated at build time
    ├── vertex.spv
    ├── vertex.metadata.json
    ├── fragment.spv
    └── fragment.metadata.json
```

Shaders are automatically compiled during build. The build system generates SPIR-V binaries and metadata files in the `compiled/` directory.

## Basic Vertex Shader

```csharp
struct Input
{
    float4 Position : TEXCOORD0;
};

struct Output
{
    float4 Position : SV_Position;
};

[shader("vertex")]
Output main(Input input)
{
    Output output;
    output.Position = input.Position;
    return output;
}
```

**Input semantics:** Use `TEXCOORD0`, `TEXCOORD1`, etc. for vertex attributes. These map to the vertex buffer configuration in `GraphicsPipelineBuilder`.

**Output semantics:** Always use `SV_Position` for the output position.

## Basic Fragment Shader

```csharp
[shader("fragment")]
float4 main() : SV_Target0
{
    return float4(1.0, 0.0, 1.0, 1.0);  // Magenta
}
```

**Output semantics:** Use `SV_Target0`, `SV_Target1`, etc. for multiple render targets (MRT). Order matches the `AddColorTarget()` calls in pipeline builder.

## Constant Buffers (Uniforms)

Fragment shader with constant buffer:

```csharp
ConstantBuffer<float4> color : register(b0, space3);

[shader("fragment")]
float4 main() : SV_Target0
{
    return color;
}
```

**Register binding:** Use `register(b{slot}, space3)` where `{slot}` is 0-3.

**Pushing data from C#:**

```csharp
// Before drawing, push uniform data
renderPass.PushFragmentUniformData(0, FColors.Magenta);  // Slot 0
```

**Available push methods:**
- `CommandBuffer.PushFragmentUniformData<T>(uint slot, T data)`
- `CommandBuffer.PushVertexUniformData<T>(uint slot, T data)`

**Slot limits:** 4 uniform slots per shader stage (0-3). Each slot can hold up to a certain size (check metadata).

## Shader Stage Attribute

Always mark entry points with the shader stage attribute:

```csharp
[shader("vertex")]    // For vertex shaders
[shader("fragment")]  // For fragment shaders
```

## Loading Shaders

**Option 1: Direct path (most common)**

```csharp
GraphicsPipeline pipeline = graphicsPipelineBuilder
    .SetShaders("shaders/vertex", "shaders/fragment")
    .Build();
```

Paths are relative to `Content/` directory and exclude the `.slang` extension. The loader automatically finds compiled `.spv` files and their metadata.

**Option 2: Load separately**

```csharp
Shader vertexShader = shaderLoader.Load("shaders/terrain_vertex");
Shader fragmentShader = shaderLoader.Load("shaders/terrain_fragment");

GraphicsPipeline pipeline = graphicsPipelineBuilder
    .SetShaders(vertexShader, fragmentShader)
    .Build();
```

Use this when you need to reuse shader objects across multiple pipelines.

## Vertex Attribute Mapping

Vertex attributes map from vertex buffer types to shader input semantics:

**C# Vertex Type:**
```csharp
.AddVertexBufferConfig<PositionColorVertex>()
```

**Shader Input:**
```csharp
struct Input
{
    float4 Position : TEXCOORD0;
    float4 Color : TEXCOORD1;
};
```

The order of `TEXCOORD` semantics must match the order of fields in the C# vertex struct.

## Multiple Render Targets (MRT)

Fragment shader with multiple outputs for deferred rendering:

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
    output.Albedo = float4(1.0, 0.0, 0.0, 1.0);
    output.Normal = float4(0.0, 1.0, 0.0, 1.0);
    output.Position = float4(0.0, 0.0, 1.0, 1.0);
    return output;
}
```

**Pipeline configuration must match:**

```csharp
.AddColorTarget(renderContextBuffers.AlbedoBuffer.Format)   // SV_Target0
.AddColorTarget(renderContextBuffers.NormalBuffer.Format)   // SV_Target1
.AddColorTarget(renderContextBuffers.PositionBuffer.Format) // SV_Target2
```

## Metadata Files

For each shader, the build generates a `.metadata.json` file:

```json
{
  "stage": "Vertex",
  "bindingLayout": {
    "bindingCounts": {
      "numSamplers": 0,
      "numStorageTextures": 0,
      "numStorageBuffers": 0
    },
    "uniformSlotSizes": {
      "slot0": 16,
      "slot1": 0,
      "slot2": 0,
      "slot3": 0
    }
  },
  "shaders": [
    {
      "format": "SpirV",
      "filename": "vertex.spv",
      "entryPoint": "main"
    }
  ]
}
```

This metadata is used by the loader to validate bindings and create GPU shader objects. You don't need to edit these files manually.

## Notes

- Shaders are compiled at build time using Slangc compiler
- Entry point is always `main`
- Shader compilation is cached based on source file hash
- Recompilation only happens when source changes
- Shader format is SPIR-V (cross-platform, works with Vulkan/Metal/D3D12)
- Always use explicit register bindings for constant buffers
- Space3 is used for constant buffers by convention
- Uniform data is pushed per-draw call before rendering
