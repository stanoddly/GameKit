using System.Runtime.InteropServices;
using GameKit.Utilities;
using SDL;

namespace GameKit.Text;

public class Font : IDisposable
{
    private FontSystem _fontSystem;
    private readonly Pointer<TTF_Font> _ttfFont;
    private readonly unsafe byte* _fontData;

    internal unsafe Font(FontSystem fontSystem, Pointer<TTF_Font> ttfFont, byte* fontData, string path, ushort size)
    {
        _fontSystem = fontSystem;
        _ttfFont = ttfFont;
        _fontData = fontData;
        Path = path;
        Size = size;
    }

    internal Pointer<TTF_Font> TtfFont => _ttfFont;
    public string Path { get; }
    public ushort Size { get; }

    internal unsafe void FreeFontData()
    {
        if (_fontData != null)
        {
            NativeMemory.Free(_fontData);
        }
    }

    public void Dispose()
    {
        _fontSystem.ReleaseFont(this);
    }
}
