# Shaders

Guide to writing and using shaders with GameKit. Shaders are written in Slang and compiled to SPIR-V at build time.

## File Structure

```
Content/shaders/
├── vertex.slang          # Source shader files
├── fragment.slang
└── .generated/           # Generated at build time
    ├── vertex.spv
    ├── vertex.metadata.json
    ├── fragment.spv
    └── fragment.metadata.json
```

Shaders are automatically compiled during build. The build system generates SPIR-V binaries and metadata files in the `.generated/` directory.

## Build Integration

Reference `GameKit.SdlangCompileTask`, import its props and targets, and declare the shaders to compile:

```xml
<ItemGroup>
    <ProjectReference Include="..\..\src\GameKit.SdlangCompileTask\GameKit.SdlangCompileTask.csproj"
                      ReferenceOutputAssembly="false" />
</ItemGroup>

<Import Project="..\..\src\GameKit.SdlangCompileTask\build\GameKit.SdlangCompileTask.props" />
<Import Project="..\..\src\GameKit.SdlangCompileTask\build\GameKit.SdlangCompileTask.targets" />

<ItemGroup>
    <SdlangShader Include="Content\shaders\*.slang" />
</ItemGroup>
```

The targets file compiles every `SdlangShader` item before `CoreCompile` and exposes the generated files as `@(SdlangShaderOutput)`. Generated files remain beside their shader sources; the compilation targets do not copy, package, or embed them. This lets each project own its complete content pipeline independently of shader compilation. `ReferenceOutputAssembly="false"` keeps the build task assemblies out of the application's output, since the task is loaded by MSBuild rather than referenced by the application.

Generated shaders are runtime content. See [Content distribution](content-distribution.md) for the loose-directory, embedded-resource, and ZIP policies, with runnable tutorials for embedding generated shaders in an assembly and publishing content in a ZIP archive.

The Slang compiler is downloaded into `GameKit.SdlangCompileLib`'s `obj/` directory and stays there. It is build-host tooling and is never copied into the output or publish directory of a project that compiles shaders. A project that needs Slang next to its own binaries (a standalone tool, or a test that calls `SdlangCompiler.CreateFromAssemblyDirectory()`) opts in with `<CopySlangToOutput>true</CopySlangToOutput>` and imports `GameKit.SdlangCompileLib`'s props and targets directly.

### Custom compilation targets

The `SdlangShader` item plus the shared target covers the normal case. To compile from somewhere else, or at a different point in the build, invoke the task directly and pass the compiler path:

```xml
<Target Name="CompileGeneratedShaders" AfterTargets="CopyFilesToOutputDirectory">
    <ItemGroup>
        <GeneratedShader Include="$(OutputPath)\Generated\*.slang" />
    </ItemGroup>
    <SdlangCompileTask InputFile="%(GeneratedShader.Identity)" SlangCompilerPath="$(SlangCompilerPath)" />
</Target>
```

Do not name a custom target `CompileSdlangShaders`; a target defined in the project overrides the imported one of the same name.

### Migrating existing projects

Earlier versions required each project to define its own target calling the task. Replace it with the `SdlangShader` item group shown above. Projects that still call the task without `SlangCompilerPath` fail the build with a message pointing here.

Remove `OutputItemType` and `OutputLogicalNamePrefix` metadata from `SdlangShader`. Projects that copy or package content should enumerate their content tree after compilation. Projects that embed generated shaders should consume `@(SdlangShaderOutput)` before `AssignTargetPaths`, as described in [Content distribution](content-distribution.md).

Only entry-point shaders belong in `SdlangShader`. Shared files consumed through `#include` or `import`, such as `common.slang`, remain excluded because they do not produce standalone runtime shaders.

Generated metadata records the normalized source dependencies and an aggregate source hash for each entry shader. Changing an included or imported source therefore recompiles every entry shader that consumes it. Slang's raw dependency file is a temporary compiler intermediate and is deleted after compilation.

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
VertexShader vertexShader = shaderLoader.LoadVertexShader("shaders/terrain_vertex");
FragmentShader fragmentShader = shaderLoader.LoadFragmentShader("shaders/terrain_fragment");

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
