using GameKit.Gpu;

namespace GameKit.Uiui;

public interface IGuiRendererConfig
{
    TextureFormat ColorTargetFormat { get; }
    DepthBufferFormat DepthBufferFormat { get; }
};