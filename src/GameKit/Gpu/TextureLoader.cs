using GameKit.Content;
using SDL;

namespace GameKit.Gpu;

public class TextureLoader: IContentLoader<Texture>
{
    private readonly GpuDevice _gpuDevice;
    private readonly Dictionary<string, Texture> _textures = new();

    public TextureLoader(GpuDevice gpuDevice)
    {
        _gpuDevice = gpuDevice;
    }

    public Texture Load(IContentManager contentManager, VirtualFileSystem fileSystem, string path)
    {
        if (_textures.TryGetValue(path, out Texture? existingTexture))
        {
            return existingTexture;
        }

        Image image = contentManager.Load<Image>(path);

        Texture texture;
        using (var memory = _gpuDevice.CreateMemoryTransfer())
        {
            texture = memory.AddTexture(image);
        }
        
        _textures[path] = texture;
        
        return texture;
    }

    public void Dispose()
    {
        foreach (Texture texture in _textures.Values)
        {
            unsafe
            {
                SDL3.SDL_ReleaseGPUTexture(_gpuDevice.SdlGpuDevice, texture.Pointer);
            }
        }
    }
}