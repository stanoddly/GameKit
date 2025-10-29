using GameKit.Utilities;
using SDL;

namespace GameKit.Text;

public class Font : IDisposable
{
    private FontSystem _fontSystem;
    private readonly Pointer<TTF_Font> _ttfFont;

    internal Font(FontSystem fontSystem, Pointer<TTF_Font> ttfFont, string path, ushort size)
    {
        _fontSystem = fontSystem;
        _ttfFont = ttfFont;
        Path = path;
        Size = size;
    }

    internal Pointer<TTF_Font> TtfFont => _ttfFont;
    public string Path { get; }
    public ushort Size { get; }

    public void Dispose()
    {
        _fontSystem.ReleaseFont(this);
    }
}