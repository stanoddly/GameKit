using System.Text.Json;
using GameKit.ShaderCommon;

namespace GameKit.SdlangCompileLib.Tests;

public class SdlangCompilerTests
{
    private const string ShaderContent = """
                                         [shader("vertex")]
                                         float4 main(float3 position : POSITION) : SV_POSITION
                                         {
                                             return float4(position, 1.0);
                                         }
                                         """;

    private const string ValidVertexShaderWithBindings = """
                                                         struct VertexInput {
                                                             float3 position : POSITION;
                                                             float2 texCoord : TEXCOORD0;
                                                         };

                                                         struct VertexOutput {
                                                             float4 position : SV_Position;
                                                             float2 texCoord : TEXCOORD0;
                                                         };

                                                         cbuffer VertexUniforms : register(b0, space1) {
                                                             float4x4 transform;
                                                         };

                                                         Texture2D<float4> myTexture : register(t0, space0);
                                                         SamplerState mySampler : register(s0, space0);

                                                         [shader("vertex")]
                                                         VertexOutput main(VertexInput input) {
                                                             VertexOutput output;
                                                             output.position = mul(transform, float4(input.position, 1.0));
                                                             output.texCoord = input.texCoord;
                                                             return output;
                                                         }
                                                         """;

    private const string VertexShaderWithSystemValueInputs = """
                                                            struct VertexInput {
                                                                float3 position : POSITION;
                                                                uint vertexId : SV_VertexID;
                                                                uint instanceId : SV_InstanceID;
                                                            };

                                                            [shader("vertex")]
                                                            float4 main(VertexInput input) : SV_POSITION
                                                            {
                                                                return float4(input.position.xy, float(input.vertexId + input.instanceId), 1.0);
                                                            }
                                                            """;

    private const string ValidFragmentShaderWithBindings = """
                                                           struct FragmentInput {
                                                               float4 position : SV_Position;
                                                               float2 texCoord : TEXCOORD0;
                                                           };

                                                           cbuffer FragmentUniforms : register(b0, space3) {
                                                               float4 tintColor;
                                                           };

                                                           Texture2D<float4> albedo : register(t0, space2);
                                                           SamplerState albedoSampler : register(s0, space2);

                                                           [shader("fragment")]
                                                           float4 main(FragmentInput input) : SV_Target {
                                                               return albedo.Sample(albedoSampler, input.texCoord) * tintColor;
                                                           }
                                                           """;

    private const string ValidComputeShaderWithBindings = """
                                                          RWTexture2D<float4> outputTexture : register(u0, space1);

                                                          ConstantBuffer<float> time : register(b0, space2);

                                                          [numthreads(8, 8, 1)]
                                                          [shader("compute")]
                                                          void main(uint3 dispatchThreadID : SV_DispatchThreadID)
                                                          {
                                                              outputTexture[dispatchThreadID.xy] = float4(time, 0.0, 0.0, 1.0);
                                                          }
                                                          """;

    private const string FragmentShaderWrongUniformSpace = """
                                                           struct FragmentInput {
                                                               float4 position : SV_Position;
                                                           };

                                                           cbuffer FragmentUniforms : register(b0, space0) {
                                                               float4 tintColor;
                                                           };

                                                           [shader("fragment")]
                                                           float4 main(FragmentInput input) : SV_Target {
                                                               return tintColor;
                                                           }
                                                           """;

    private const string VertexShaderWrongUniformSpace = """
                                                         cbuffer VertexUniforms : register(b0, space3) {
                                                             float4x4 transform;
                                                         };

                                                         [shader("vertex")]
                                                         float4 main(float3 position : POSITION) : SV_POSITION {
                                                             return mul(transform, float4(position, 1.0));
                                                         }
                                                         """;

    private const string FragmentShaderWrongTextureSpace = """
                                                           struct FragmentInput {
                                                               float4 position : SV_Position;
                                                               float2 texCoord : TEXCOORD0;
                                                           };

                                                           Texture2D<float4> albedo : register(t0, space0);
                                                           SamplerState albedoSampler : register(s0, space0);

                                                           [shader("fragment")]
                                                           float4 main(FragmentInput input) : SV_Target {
                                                               return albedo.Sample(albedoSampler, input.texCoord);
                                                           }
                                                           """;

    private const string FragmentShaderWrongIndexOrder = """
                                                         struct FragmentInput {
                                                             float4 position : SV_Position;
                                                             float2 texCoord : TEXCOORD0;
                                                         };

                                                         StructuredBuffer<float4> myData : register(t0, space2);
                                                         Texture2D<float4> albedo : register(t1, space2);
                                                         SamplerState albedoSampler : register(s0, space2);

                                                         [shader("fragment")]
                                                         float4 main(FragmentInput input) : SV_Target {
                                                             return albedo.Sample(albedoSampler, input.texCoord) + myData[0];
                                                         }
                                                         """;

    private const string VertexShaderMismatchedSamplerIndex = """
                                                              struct VertexInput {
                                                                  float3 position : POSITION;
                                                                  float2 texCoord : TEXCOORD0;
                                                              };

                                                              struct VertexOutput {
                                                                  float4 position : SV_Position;
                                                                  float4 color : COLOR0;
                                                              };

                                                              Texture2D<float4> albedo : register(t0, space0);
                                                              SamplerState albedoSampler : register(s1, space0);

                                                              [shader("vertex")]
                                                              VertexOutput main(VertexInput input) {
                                                                  VertexOutput output;
                                                                  output.position = float4(input.position, 1.0);
                                                                  output.color = albedo.SampleLevel(albedoSampler, input.texCoord, 0.0);
                                                                  return output;
                                                              }
                                                              """;

    private const string FragmentShaderMismatchedSamplerIndex = """
                                                                struct FragmentInput {
                                                                    float4 position : SV_Position;
                                                                    float2 texCoord : TEXCOORD0;
                                                                };

                                                                Texture2D<float4> albedo : register(t0, space2);
                                                                SamplerState albedoSampler : register(s1, space2);

                                                                [shader("fragment")]
                                                                float4 main(FragmentInput input) : SV_Target {
                                                                    return albedo.Sample(albedoSampler, input.texCoord);
                                                                }
                                                                """;

    private const string ComputeShaderMismatchedSamplerIndex = """
                                                               Texture2D<float4> inputTexture : register(t0, space0);
                                                               SamplerState inputSampler : register(s1, space0);
                                                               RWTexture2D<float4> outputTexture : register(u0, space1);

                                                               [numthreads(8, 8, 1)]
                                                               [shader("compute")]
                                                               void main(uint3 dispatchThreadID : SV_DispatchThreadID)
                                                               {
                                                                   float2 uv = float2(dispatchThreadID.xy) / float2(8.0, 8.0);
                                                                   outputTexture[dispatchThreadID.xy] = inputTexture.SampleLevel(inputSampler, uv, 0.0);
                                                               }
                                                               """;

    private string _testDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "SdlangCompilerTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    [Test]
    public void CompileShader_CreatesMetadataFile()
    {
        // Arrange
        string shaderPath = Path.Combine(_testDir, "test_shader.slang");
        File.WriteAllText(shaderPath, ShaderContent);

        SdlangCompiler compiler = new SdlangCompiler();

        // Act
        compiler.Compile([shaderPath], force: true);

        // Assert
        string metadataPath = Path.Combine(_testDir, ".generated", "test_shader.metadata.json");
        Assert.That(File.Exists(metadataPath), Is.True, "Metadata file should be created");

        string json = File.ReadAllText(metadataPath);
        
        VertexShaderMetadataDto? metadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.VertexShaderMetadataDto);

        Assert.That(metadata, Is.Not.Null);
        Assert.That(metadata.Stage, Is.EqualTo(ShaderStageDto.Vertex));
        Assert.That(metadata.Shaders.Count, Is.GreaterThan(0));
        Assert.That(metadata.SourceHash, Is.Not.Empty);
        Assert.That(metadata.SystemValueInputs.UsesVertexId, Is.False);
        Assert.That(metadata.SystemValueInputs.UsesInstanceId, Is.False);

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.TryGetProperty("threadCountX", out JsonElement _), Is.False);
    }

    [Test]
    public void CompileShader_VertexShaderWithSystemValueInputs_CreatesSystemValueMetadata()
    {
        string shaderPath = Path.Combine(_testDir, "system_values.slang");
        File.WriteAllText(shaderPath, VertexShaderWithSystemValueInputs);

        SdlangCompiler compiler = new SdlangCompiler();
        compiler.Compile([shaderPath], force: true);

        string metadataPath = Path.Combine(_testDir, ".generated", "system_values.metadata.json");
        string json = File.ReadAllText(metadataPath);

        VertexShaderMetadataDto? metadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.VertexShaderMetadataDto);

        Assert.That(metadata, Is.Not.Null);
        Assert.That(metadata.SystemValueInputs.UsesVertexId, Is.True);
        Assert.That(metadata.SystemValueInputs.UsesInstanceId, Is.True);
    }

    [Test]
    public void CompileShader_ValidVertexShaderWithBindings_Succeeds()
    {
        string shaderPath = Path.Combine(_testDir, "valid_vertex.slang");
        File.WriteAllText(shaderPath, ValidVertexShaderWithBindings);

        SdlangCompiler compiler = new SdlangCompiler();
        compiler.Compile([shaderPath], force: true);

        string metadataPath = Path.Combine(_testDir, ".generated", "valid_vertex.metadata.json");
        Assert.That(File.Exists(metadataPath), Is.True);
    }

    [Test]
    public void CompileShader_ValidFragmentShaderWithBindings_Succeeds()
    {
        string shaderPath = Path.Combine(_testDir, "valid_fragment.slang");
        File.WriteAllText(shaderPath, ValidFragmentShaderWithBindings);

        SdlangCompiler compiler = new SdlangCompiler();
        compiler.Compile([shaderPath], force: true);

        string metadataPath = Path.Combine(_testDir, ".generated", "valid_fragment.metadata.json");
        Assert.That(File.Exists(metadataPath), Is.True);
    }

    [Test]
    public void CompileShader_ValidComputeShaderWithBindings_CreatesComputeMetadata()
    {
        string shaderPath = Path.Combine(_testDir, "valid_compute.slang");
        File.WriteAllText(shaderPath, ValidComputeShaderWithBindings);

        SdlangCompiler compiler = new SdlangCompiler();
        compiler.Compile([shaderPath], force: true);

        string metadataPath = Path.Combine(_testDir, ".generated", "valid_compute.metadata.json");
        Assert.That(File.Exists(metadataPath), Is.True);

        string json = File.ReadAllText(metadataPath);
        ComputeShaderMetadataDto? metadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.ComputeShaderMetadataDto);

        Assert.That(metadata, Is.Not.Null);
        Assert.That(metadata.Stage, Is.EqualTo(ShaderStageDto.Compute));
        Assert.That(metadata.ThreadCountX, Is.EqualTo(8));
        Assert.That(metadata.ThreadCountY, Is.EqualTo(8));
        Assert.That(metadata.ThreadCountZ, Is.EqualTo(1));
        Assert.That(metadata.Shaders.Count, Is.GreaterThan(0));
    }

    [Test]
    public void CompileShader_FragmentShaderWrongUniformSpace_ThrowsValidationException()
    {
        string shaderPath = Path.Combine(_testDir, "invalid_fragment.slang");
        File.WriteAllText(shaderPath, FragmentShaderWrongUniformSpace);

        SdlangCompiler compiler = new SdlangCompiler();

        ShaderBindingValidationException? ex = Assert.Throws<ShaderBindingValidationException>(() =>
            compiler.Compile([shaderPath], force: true));

        Assert.That(ex.Message, Does.Contain("space 0"));
        Assert.That(ex.Message, Does.Contain("space 3"));
        Assert.That(ex.Message, Does.Contain("uniform buffers"));
    }

    [Test]
    public void CompileShader_VertexShaderWrongUniformSpace_ThrowsValidationException()
    {
        string shaderPath = Path.Combine(_testDir, "invalid_vertex.slang");
        File.WriteAllText(shaderPath, VertexShaderWrongUniformSpace);

        SdlangCompiler compiler = new SdlangCompiler();

        ShaderBindingValidationException? ex = Assert.Throws<ShaderBindingValidationException>(() =>
            compiler.Compile([shaderPath], force: true));

        Assert.That(ex.Message, Does.Contain("space 3"));
        Assert.That(ex.Message, Does.Contain("space 1"));
        Assert.That(ex.Message, Does.Contain("uniform buffers"));
    }

    [Test]
    public void CompileShader_FragmentShaderWrongTextureSpace_ThrowsValidationException()
    {
        string shaderPath = Path.Combine(_testDir, "invalid_texture.slang");
        File.WriteAllText(shaderPath, FragmentShaderWrongTextureSpace);

        SdlangCompiler compiler = new SdlangCompiler();

        ShaderBindingValidationException? ex = Assert.Throws<ShaderBindingValidationException>(() =>
            compiler.Compile([shaderPath], force: true));

        Assert.That(ex.Message, Does.Contain("space 0"));
        Assert.That(ex.Message, Does.Contain("space 2"));
    }

    [Test]
    public void CompileShader_FragmentShaderWrongIndexOrder_ThrowsValidationException()
    {
        string shaderPath = Path.Combine(_testDir, "invalid_order.slang");
        File.WriteAllText(shaderPath, FragmentShaderWrongIndexOrder);

        SdlangCompiler compiler = new SdlangCompiler();

        ShaderBindingValidationException? ex = Assert.Throws<ShaderBindingValidationException>(() =>
            compiler.Compile([shaderPath], force: true));

        Assert.That(ex.Message, Does.Contain("index"));
    }

    [TestCase(VertexShaderMismatchedSamplerIndex)]
    [TestCase(FragmentShaderMismatchedSamplerIndex)]
    [TestCase(ComputeShaderMismatchedSamplerIndex)]
    public void CompileShader_MismatchedSamplerTextureIndex_ThrowsValidationException(string shaderContent)
    {
        string shaderPath = CreateTemporaryShaderFile(shaderContent);

        SdlangCompiler compiler = new SdlangCompiler();

        ShaderBindingValidationException? ex = Assert.Throws<ShaderBindingValidationException>(() =>
            compiler.Compile([shaderPath], force: true));

        Assert.That(ex.Message, Does.Contain("same index and space"));
    }

    private const string FragmentShaderWithStructStorageBuffer = """
                                                                 struct FragmentInput {
                                                                     float4 position : SV_Position;
                                                                 };

                                                                 struct MyData {
                                                                     float4 position;
                                                                     float2 texCoord;
                                                                     float intensity;
                                                                 };

                                                                 StructuredBuffer<MyData> dataBuffer : register(t0, space2);

                                                                 [shader("fragment")]
                                                                 float4 main(FragmentInput input) : SV_Target {
                                                                     MyData d = dataBuffer[0];
                                                                     return d.position * d.intensity;
                                                                 }
                                                                 """;

    private const string FragmentShaderWithPrimitiveStorageBuffer = """
                                                                     struct FragmentInput {
                                                                         float4 position : SV_Position;
                                                                     };

                                                                     StructuredBuffer<float4> colorBuffer : register(t0, space2);

                                                                     [shader("fragment")]
                                                                     float4 main(FragmentInput input) : SV_Target {
                                                                         return colorBuffer[0];
                                                                     }
                                                                     """;

    [Test]
    public void CompileShader_FragmentShaderWithStructStorageBuffer_StoresElementSize()
    {
        string shaderPath = CreateTemporaryShaderFile(FragmentShaderWithStructStorageBuffer);
        SdlangCompiler compiler = new SdlangCompiler();
        compiler.Compile([shaderPath], force: true);

        string metadataPath = Path.ChangeExtension(
            Path.Combine(_testDir, ".generated", Path.GetFileName(shaderPath)),
            ".metadata.json");
        string json = File.ReadAllText(metadataPath);

        FragmentShaderMetadataDto? metadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.FragmentShaderMetadataDto);

        Assert.That(metadata, Is.Not.Null);
        // MyData: float4 (16) + float2 (8) + float (4) = 28 bytes at slot 0
        Assert.That(metadata.BindingLayout.StorageBufferElementSizes.Slot0, Is.EqualTo(28u));
        Assert.That(metadata.BindingLayout.StorageBufferElementSizes.Slot1, Is.EqualTo(0u));
    }

    [Test]
    public void CompileShader_FragmentShaderWithPrimitiveStorageBuffer_StoresElementSize()
    {
        string shaderPath = CreateTemporaryShaderFile(FragmentShaderWithPrimitiveStorageBuffer);
        SdlangCompiler compiler = new SdlangCompiler();
        compiler.Compile([shaderPath], force: true);

        string metadataPath = Path.ChangeExtension(
            Path.Combine(_testDir, ".generated", Path.GetFileName(shaderPath)),
            ".metadata.json");
        string json = File.ReadAllText(metadataPath);

        FragmentShaderMetadataDto? metadata = JsonSerializer.Deserialize(json, ShaderMetadataJsonContext.Default.FragmentShaderMetadataDto);

        Assert.That(metadata, Is.Not.Null);
        // float4: 4 floats * 4 bytes = 16 bytes at slot 0
        Assert.That(metadata.BindingLayout.StorageBufferElementSizes.Slot0, Is.EqualTo(16u));
    }

    private string CreateTemporaryShaderFile(string shaderContent)
    {
        string filename = Path.ChangeExtension(Path.GetRandomFileName(), ".slang");
        string shaderPath = Path.Combine(_testDir, filename);
        File.WriteAllText(shaderPath, shaderContent);
        return shaderPath;
    }
}
