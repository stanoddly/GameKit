using System.Runtime.InteropServices;
using GameKit.Utilities;
using SDL;

namespace GameKit.Text;

public class Font : IDisposable
{
    private FontSystem _fontSystem;
    private readonly Pointer<TTF_Font> _ttfFont;
    private readonly GCHandle _fontDataHandle;

    internal Font(FontSystem fontSystem, Pointer<TTF_Font> ttfFont, GCHandle fontDataHandle, string path, ushort size)
    {
        _fontSystem = fontSystem;
        _ttfFont = ttfFont;
        _fontDataHandle = fontDataHandle;
        Path = path;
        Size = size;
    }

    internal Pointer<TTF_Font> TtfFont => _ttfFont;
    public string Path { get; }
    public ushort Size { get; }

    internal void FreeFontData()
    {
        if (_fontDataHandle.IsAllocated)
        {
            _fontDataHandle.Free();
        }
    }

    public void Dispose()
    {
        _fontSystem.ReleaseFont(this);
    }
}