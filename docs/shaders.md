# Shaders

Guide to writing and using shaders with GameKit. Shaders are written in Slang and compiled to SPIR-V, DXIL, and MSL at build time.

## File Structure

```
Content/shaders/
├── vertex.slang          # Source shader files
├── fragment.slang
└── .generated/           # Generated at build time
    ├── vertex.spv
    ├── vertex.dxil
    ├── vertex.metal
    ├── vertex.metadata.json
    ├── fragment.spv
    ├── fragment.dxil
    ├── fragment.metal
    └── fragment.metadata.json
```

Shaders are automatically compiled during build. The build system generates SPIR-V binaries for Vulkan, DXIL binaries for Direct3D 12, MSL source for Metal, and metadata files in the `.generated/` directory.

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

The Slang compiler is downloaded from [`stanoddly/slang-dxc-bundle`](https://github.com/stanoddly/slang-dxc-bundle) into `GameKit.SdlangCompileLib`'s `obj/` directory and stays there. The distribution includes the DXC downstream compiler required for DXIL output and provides bundles for every supported shader-compilation host: Linux x64/ARM64, Windows x64, and macOS x64/ARM64. DXIL generation is therefore required on every supported host and is never silently omitted. `GameKit.SdlangCompileLib` alone owns the shared download and extraction. Slang and DXC are build-host tooling and are never copied into the output or publish directory of a project that only compiles shaders. A project that needs Slang next to its own binaries opts in with `<CopySlangToOutput>true</CopySlangToOutput>` and imports `GameKit.SdlangCompileLib`'s props and targets directly. The property copies the complete Slang distribution, including DXC, to build and publish outputs. They remain external when publishing a single-file application because `slangc` must be executable by path.

Standalone applications should call `SdlangCompiler.CreateFromApplicationDirectory()`. It locates Slang relative to the application directory and supports single-file publishing. `SdlangCompiler.CreateFromAssemblyDirectory()` locates Slang relative to `GameKit.SdlangCompileLib.dll` and requires assembly files on disk, so it is not compatible with single-file applications. Build integrations should pass `$(SlangCompilerPath)` to the `SdlangCompiler` constructor instead of using either factory.

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

Paths are relative to the `Content/` directory and exclude the `.slang` extension. The loader reads the metadata and selects the first compiled format supported by the active GPU backend.

**Option 2: Load separately**

```csharp
VertexShader vertexShader = shaderLoader.LoadVertexShader("shaders/terrain_vertex");
FragmentShader fragmentShader = shaderLoader.LoadFragmentShader("shaders/terrain_fragment");

GraphicsPipeline pipeline = graphicsPipelineBuilder
    .SetShaders(vertexShader, fragmentShader)
    .Build();
```

Use this when you need to reuse shader objects across multiple pipelines.

## GPU Backend Selection

GameKit lets SDL choose the GPU backend automatically by default. Register `GameKitConfig` before building the application to request a specific backend:

```csharp
builder.AddSingleton(new GameKitConfig(GpuBackend: GpuBackend.Direct3D12));
```

`GpuBackend` supports `Automatic`, `Vulkan`, `Direct3D12`, and `Metal`. An explicit choice is passed to SDL as `vulkan`, `direct3d12`, or `metal`; device creation fails if that driver is unavailable. Windows device creation advertises both SPIR-V and DXIL, allowing automatic selection between Vulkan and Direct3D 12. Vulkan-specific device options remain enabled whenever Vulkan can be selected.

The selected SDL driver is available from `GpuDevice.Driver` for diagnostics.

### Manual Direct3D 12 validation

On a GPU-equipped Windows system, add the following registrations to each application under test:

```csharp
builder.AddSingleton(new GameKitConfig(
    EnableGpuValidation: true,
    GpuBackend: GpuBackend.Direct3D12));

builder.OnStart((GpuDevice gpuDevice) =>
{
    if (gpuDevice.Driver != "direct3d12")
    {
        throw new InvalidOperationException($"Expected direct3d12, got {gpuDevice.Driver}");
    }
});
```

Run these representative workloads and confirm that each renders without SDL GPU validation errors:

```shell
dotnet run --project tutorials/GameKit.Tutorials.Triangle
dotnet run --project tutorials/GameKit.Tutorials.ImageLoading
dotnet run --project tutorials/GameKit.Tutorials.StorageBuffer
dotnet run --project tutorials/GameKit.Tutorials.ComputeShader
```

Together these cover basic graphics, texture/sampler bindings, storage buffers, and compute dispatch.

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
    },
    {
      "format": "Dxil",
      "filename": "vertex.dxil",
      "entryPoint": "main"
    },
    {
      "format": "Msl",
      "filename": "vertex.metal",
      "entryPoint": "main_0"
    }
  ]
}
```

This metadata is used by the loader to validate bindings and create GPU shader objects. You don't need to edit these files manually.

## Notes

- Shaders are compiled at build time using Slangc compiler
- The source entry point is always `main`; generated MSL exposes it as `main_0`
- Shader compilation is cached based on the source hash, Slang version, and expected target formats
- SPIR-V is used by Vulkan, DXIL by Direct3D 12, and MSL by Metal
- Always use explicit register bindings for constant buffers
- Space3 is used for constant buffers by convention
- Uniform data is pushed per-draw call before rendering
