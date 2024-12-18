using System.Text.Json;
using GameKit.Content;
using MessagePack;

namespace GameKit.Shaders;

public class ShaderPackLoader: IContentLoader<ShaderPack>
{
    private VirtualFileSystem _fileSystem;

    public ShaderPackLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    private ShaderPack LoadJson(string path)
    {
        using Stream stream = _fileSystem.GetFile(path).Open();
        // reflection free deserialization
        ShaderPack? shaderPack = JsonSerializer.Deserialize(stream, ShaderMetaJsonContext.Default.ShaderPack);

        if (shaderPack == null)
        {
            // TODO: improve
            throw new Exception();
        }

        return shaderPack;
    }

    private ShaderPack LoaderMsgPack(string path)
    {
        using Stream stream = _fileSystem.GetFile(path).Open();

        ShaderPackDtoMsgPack msgPack = MessagePackSerializer.Deserialize<ShaderPackDtoMsgPack>(stream);

        List<ShaderInstance> shaderInstances = new List<ShaderInstance>();
        foreach (ShaderInstanceDtoMsgPack shaderInstanceDto in msgPack.Shaders)
        {
            shaderInstances.Add(new ShaderInstance{ Content = shaderInstanceDto.Content, EntryPoint = shaderInstanceDto.EntryPoint, Format = shaderInstanceDto.Format });
        }

        var resourcesDto = msgPack.Resources;

        ShaderResources shaderResources = new ShaderResources(resourcesDto.Samplers, resourcesDto.StorageTextures,
            resourcesDto.StorageBuffers, resourcesDto.UniformBuffers);

        return new ShaderPack
        {
            Stage = msgPack.Stage,
            Resources = shaderResources,
            Shaders = shaderInstances
        };
    }

    public ShaderPack Load(string path)
    {
        if (path.EndsWith(".json"))
        {
            return LoadJson(path);
        }
        
        return LoaderMsgPack(path);
    }
}
