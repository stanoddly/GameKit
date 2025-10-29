namespace GameKit.Gpu;

public enum DepthBufferFormat: ushort
{
    // None is "0" and obviously "default" too
    None = TextureFormat.None,
    Depth16 = TextureFormat.D16Unorm,
    Depth24 = TextureFormat.D24Unorm, 
    Depth32 = TextureFormat.D32Float,
    Depth24Stencil8 = TextureFormat.D24UnormS8Uint,
    Depth32Stencil8 = TextureFormat.D32FloatS8Uint
}
