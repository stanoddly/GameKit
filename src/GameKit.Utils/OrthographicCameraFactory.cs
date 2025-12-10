using GameKit;
using GameKit.Common;

namespace GameKit.Utils;

public static class OrthographicCameraFactory
{
    public static Camera Create(IWindow window, IViewConfiguration viewConfiguration)
    {
        ShortSize windowSize = window.RenderSizeInPixels;

        float width = windowSize.Width / viewConfiguration.PixelsPerUnit;
        float height = windowSize.Height / viewConfiguration.PixelsPerUnit;

        return new OrthographicCamera
        {
            Width = width,
            Height = height,
            NearPlane = 0.1f,
            FarPlane = 1000f
        };
    }
}