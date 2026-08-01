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

The targets file declares every generated output as an MSBuild item before target paths are assigned, then compiles every `SdlangShader` item before `CoreCompile`. Generated files are copied to the same project-relative paths in build and publish output. For example, outputs for `Content/shaders/vertex.slang` are copied under `Content/shaders/.generated`. Declaring the expected output paths independently of their existence makes clean builds work even though `.generated` does not exist when MSBuild evaluates the project. It also lets publish reuse an existing build without invoking the shader compiler. `ReferenceOutputAssembly="false"` keeps the build task assemblies out of the application's output, since the task is loaded by MSBuild rather than referenced by the application.

Set `OutputItemType` to `None` when another build target owns the generated files, such as a target that copies an entire content directory after compilation:

```xml
<SdlangShader Include="..\Game.Executable\Content\shaders\*.slang">
    <OutputItemType>None</OutputItemType>
</SdlangShader>
```

The shaders are still compiled beside their sources, but their generated files are not copied or embedded by the shared targets. Enumerate files inside the owning copy target so files created during the build are included.

Generated files can instead be embedded without also copying them as standalone content:

```xml
<SdlangShader Include="Content\shaders\*.slang">
    <OutputItemType>EmbeddedResource</OutputItemType>
    <OutputLogicalNamePrefix>shaders/.generated/</OutputLogicalNamePrefix>
</SdlangShader>
```

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

Projects that already declare entry-point shaders with `SdlangShader` need no changes. Generated files use the normal content flow automatically; do not add `CopyToOutputDirectory`, `CopyToPublishDirectory`, or a custom copy target.

Remove any existing target that copies files from `.generated` into build or publish output. The shared targets now own that behavior.

Earlier versions required each project to define its own target calling the task. Replace it with the `SdlangShader` item group shown above. Projects that still call the task without `SlangCompilerPath` fail the build with a message pointing here.

If a project embeds generated files with an `EmbeddedResource` glob over `.generated`, remove that glob and set `OutputItemType` and `OutputLogicalNamePrefix` on `SdlangShader` as shown above. This lets clean builds declare the resources before the generated files exist.

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
