using System.Text.Json;
using GameKit.Content;

namespace GameKit.Shaders;

public class ShaderMetadataLoader: IContentLoader<ShaderMetadata>
{
    private readonly VirtualFileSystem _fileSystem;

    public ShaderMetadataLoader(VirtualFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ShaderMetadata Load(string path)
    {
        using Stream stream = _fileSystem.GetFile(path).Open();
        // reflection free deserialization
        ShaderMetadata? shaderMetadata = JsonSerializer.Deserialize(stream, ShaderMetadataJsonContext.Default.ShaderMetadata);

        if (shaderMetadata == null)
        {
            // TODO: improve
            throw new Exception();
        }

        return shaderMetadata;
    }
}
