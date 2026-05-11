using GameKit.ShaderCommon;

namespace GameKit.Shaders;

public class ComputeShaderMetadata
{
    public required ShaderBindingLayout BindingLayout { get; init; }
    public required List<ShaderInstance> Shaders { get; init; }
    public required uint ThreadCountX { get; init; }
    public required uint ThreadCountY { get; init; }
    public required uint ThreadCountZ { get; init; }
}
