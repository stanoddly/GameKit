using System.Runtime.Serialization;

namespace GameKit.ShaderCommon;

public enum ShaderStageDto
{
    [EnumMember(Value = "vertex")]
    Vertex = 0,

    [EnumMember(Value = "fragment")]
    Fragment = 1,

    [EnumMember(Value = "compute")]
    Compute = 2
}

public enum ShaderFormatDto
{
    [EnumMember(Value = "private")]
    Private = 0,

    [EnumMember(Value = "spirv")]
    SpirV = 1,

    [EnumMember(Value = "dxbc")]
    Dxbc = 2,

    [EnumMember(Value = "dxil")]
    Dxil = 3,

    [EnumMember(Value = "msl")]
    Msl = 4,

    [EnumMember(Value = "metallib")]
    MetalLib = 5
}

public record ShaderInstanceDto(ShaderFormatDto Format, string Filename, string EntryPoint);

public record ShaderMetadataHeaderDto
{
    public required ShaderStageDto Stage { get; init; }
    public required string SourceHash { get; init; }
    public string? SlangVersion { get; init; }
}

public record VertexShaderMetadataDto
{
    public ShaderStageDto Stage { get; init; } = ShaderStageDto.Vertex;
    public required ShaderBindingLayout BindingLayout { get; init; }
    public required List<ShaderInstanceDto> Shaders { get; init; }
    public required string SourceHash { get; init; }
    public string? SlangVersion { get; init; }
}

public record FragmentShaderMetadataDto
{
    public ShaderStageDto Stage { get; init; } = ShaderStageDto.Fragment;
    public required ShaderBindingLayout BindingLayout { get; init; }
    public required List<ShaderInstanceDto> Shaders { get; init; }
    public required string SourceHash { get; init; }
    public string? SlangVersion { get; init; }
}

public record ComputeShaderMetadataDto
{
    public ShaderStageDto Stage { get; init; } = ShaderStageDto.Compute;
    public required ShaderBindingLayout BindingLayout { get; init; }
    public required List<ShaderInstanceDto> Shaders { get; init; }
    public required string SourceHash { get; init; }
    public string? SlangVersion { get; init; }
    public required uint ThreadCountX { get; init; }
    public required uint ThreadCountY { get; init; }
    public required uint ThreadCountZ { get; init; }
}
