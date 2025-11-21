using System.Runtime.Serialization;

namespace GameKit.ShaderCommon;

public enum ShaderStageDto
{
    [EnumMember(Value = "vertex")]
    Vertex = 0,

    [EnumMember(Value = "fragment")]
    Fragment = 1
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

public record ShaderMetadataDto(ShaderStageDto Stage, ShaderBindingLayout BindingLayout, List<ShaderInstanceDto> Shaders, string SourceHash);
