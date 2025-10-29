using GameKit.Common;

namespace GameKit.Uiui;

public readonly record struct ResolutionInfo(ushort ScaleFactor, ShortSize BaseResolution, ShortSize FinalResolution, ShortRectangle WidgetBounds);

public class GuiResolutionProvider
{
    //private const int MinBaseHeight = 360;
    //private const int MaxBaseHeight = 512;

    private const int MinBaseHeight = 720;
    private const int MaxBaseHeight = 1080;

    public ResolutionInfo ResolutionInfo { get; private set; }

    public GuiResolutionProvider(IWindow window)
    {
        ResolutionInfo = CalculateResolution(window.RenderSizeInPixels);
    }

    private static int CalculateBaseHeight(int screenHeight, int minBaseHeight, int maxBaseHeight)
    {
        for (int baseHeight = maxBaseHeight; baseHeight >= minBaseHeight; baseHeight--)
        {
            if (screenHeight % baseHeight == 0)
            {
                return baseHeight;
            }
        }

        int bestBaseHeight = minBaseHeight;
        int minRemainder = screenHeight % minBaseHeight;

        for (int baseHeight = minBaseHeight + 1; baseHeight <= maxBaseHeight; baseHeight++)
        {
            int remainder = screenHeight % baseHeight;
            if (remainder < minRemainder)
            {
                minRemainder = remainder;
                bestBaseHeight = baseHeight;
            }
        }

        return bestBaseHeight;
    }

    private static ResolutionInfo CalculateResolution(ShortSize screenSize)
    {
        int baseHeight = CalculateBaseHeight(screenSize.Height, MinBaseHeight, MaxBaseHeight);
        ushort scale = (ushort)(screenSize.Height / baseHeight);

        int baseWidth = (int)Math.Round((double)screenSize.Width / scale);

        int finalWidth = baseWidth * scale;
        int finalHeight = baseHeight * scale;

        int widthOffset = (screenSize.Width - finalWidth) / 2 / scale;
        var widgetBounds = new ShortRectangle((short)widthOffset, 0, (short)baseWidth, (short)baseHeight);

        return new ResolutionInfo(
            scale, 
            new ShortSize((ushort)baseWidth, (ushort)baseHeight), 
            new ShortSize((ushort)finalWidth, (ushort)finalHeight),
            widgetBounds
        );
    }
}
