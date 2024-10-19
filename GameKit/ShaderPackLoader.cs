using System.Text.Json;
using GameKit.Content;

namespace GameKit;

public class ShaderPackLoader: IContentLoader<ShaderPack>
{
    public ShaderPack Load(IContentManager contentManager, VirtualFileSystem virtualFileSystem, string path)
    {
        using Stream stream = virtualFileSystem.GetFile(path).Open();
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