using System.IO.Abstractions;
using System.Text.Json;
using GameKit.Content;
using FileSystem = GameKit.Content.FileSystem;

namespace GameKit;

public class ShaderPackLoader: IContentLoader<ShaderPack>
{
    public ShaderPack Load(IContentManager contentManager, FileSystem fileSystem, string path)
    {
        using Stream stream = fileSystem.GetFile(path).Open();
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