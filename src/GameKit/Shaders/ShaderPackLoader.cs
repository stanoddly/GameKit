using System.Text.Json;
using GameKit.Content;

namespace GameKit.Shaders;

public class ShaderPackLoader: IContentLoader<ShaderPack>
{
    private VirtualFileSystem _fileSystem;

    public ShaderPackLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ShaderPack Load(string path)
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
}
