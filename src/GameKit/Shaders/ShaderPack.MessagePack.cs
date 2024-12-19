using MessagePack;

namespace GameKit.Shaders;

[MessagePackObject]
public readonly record struct ShaderResourcesDtoMsgPack(
    [property: Key("samplers")] int Samplers = 0,
    [property: Key("storageTextures")] int StorageTextures = 0,
    [property: Key("storageBuffers")] int StorageBuffers = 0,
    [property: Key("uniformBuffers")] int UniformBuffers = 0);

[MessagePackObject]
public class ShaderInstanceDtoMsgPack
{
    [Key("format")]
    public required ShaderFormat Format { get; init; }

    [Key("content")]
    public required byte[] Content { get; init; }

    [Key("entryPoint")]
    public required string EntryPoint { get; init; }
}

[MessagePackObject]
public class ShaderPackDtoMsgPack
{
    [Key("stage")]
    public required ShaderStage Stage { get; init; }
    [Key("resources")]
    public required ShaderResourcesDtoMsgPack Resources { get; init; }
    [Key("shaders")]
    public required List<ShaderInstanceDtoMsgPack> Shaders { get; init; }
}
