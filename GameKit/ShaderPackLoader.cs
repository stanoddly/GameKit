using System.IO.Abstractions;
using System.Text.Json;

namespace GameKit;

public class ShaderPackLoader: ContentLoader<ShaderPack>
{
    public override ShaderPack Load(IFileSystem fileSystem, string path)
    {
        using Stream stream = fileSystem.FileStream.New(path, FileMode.Open, FileAccess.Read, FileShare.Read);
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