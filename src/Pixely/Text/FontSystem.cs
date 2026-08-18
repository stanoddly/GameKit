using System.Runtime.InteropServices;
using Pixely.Content;
using Pixely.Gpu;
using Pixely.Utilities;
using SDL;

namespace Pixely.Text;

internal class FontSystem: IFontSystem, IUpdatable
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly VirtualFileSystem _fileSystem;
    private readonly List<Font> _fonts = new();
    private readonly Dictionary<(string path, ushort size, FontRasterizationMode rasterizationMode, FontHintingMode hintingMode), Font> _fontCache = new();
    private readonly Dictionary<(string text, Font font), (WeakReference<TextSpriteAsset> WeakRef, Texture Texture)> _textSpriteCache = new();

    private FontSystem(GpuMemorySystem gpuMemorySystem, VirtualFileSystem fileSystem)
    {
        _gpuMemorySystem = gpuMemorySystem;
        _fileSystem = fileSystem;
    }

    public static FontSystem Create(GpuMemorySystem gpuMemorySystem, VirtualFileSystem fileSystem)
    {
        if (!SDL3_ttf.TTF_Init())
        {
            SdlError.Throw(nameof(SDL3_ttf.TTF_Init));
        }

        return new FontSystem(gpuMemorySystem, fileSystem);
    }

    public Font Load(
        string path,
        ushort size,
        FontRasterizationMode rasterizationMode = FontRasterizationMode.Blended,
        FontHintingMode hintingMode = FontHintingMode.Normal)
    {
        (string path, ushort size, FontRasterizationMode rasterizationMode, FontHintingMode hintingMode) cacheKey =
            (path, size, rasterizationMode, hintingMode);
        if (_fontCache.TryGetValue(cacheKey, out Font? cachedFont))
        {
            return cachedFont;
        }

        VirtualFile fontFile = _fileSystem.GetFile(path);

        using Stream stream = fontFile.Open();
        int fontDataLength = (int)stream.Length;

        unsafe
        {
            byte* nativeFontData = (byte*)NativeMemory.Alloc((nuint)fontDataLength);
            stream.ReadExactly(new Span<byte>(nativeFontData, fontDataLength));

            Pointer<SDL_IOStream> sdlStream = SDL3.SDL_IOFromConstMem((IntPtr)nativeFontData, (UIntPtr)fontDataLength);
            SdlError.ThrowOnNull(sdlStream, nameof(SDL3.SDL_IOFromConstMem));

            Pointer<TTF_Font> ttfFont = SDL3_ttf.TTF_OpenFontIO(sdlStream, true, size);
            SdlError.ThrowOnNull(ttfFont, nameof(SDL3_ttf.TTF_OpenFontIO));

            SDL3_ttf.TTF_SetFontHinting(ttfFont, ToSdlHintingMode(hintingMode));

            Font font = new Font(this, ttfFont, nativeFontData, path, size, rasterizationMode, hintingMode);
            _fonts.Add(font);
            _fontCache[cacheKey] = font;

            return font;
        }
    }

    public TextSpriteAsset CreateTextSprite(string text, Font font)
    {
        var cacheKey = (text, font);
        if (_textSpriteCache.TryGetValue(cacheKey, out var cached))
        {
            if (cached.WeakRef.TryGetTarget(out TextSpriteAsset? cachedSprite))
            {
                return cachedSprite;
            }

            ShortSize size = cached.Texture.Size;
            ShortRectangle imageRegion = new(0, 0, size.Width, size.Height);
            TextSpriteAsset textSprite = new(cached.Texture, imageRegion);

            _textSpriteCache[cacheKey] = (new WeakReference<TextSpriteAsset>(textSprite), cached.Texture);
            return textSprite;
        }

        Pointer<SDL_Surface> surface = RenderTextToSurface(text, font);
        try
        {
            Texture texture = CreateTextureFromSurface(surface);
            ShortSize size = texture.Size;
            ShortRectangle imageRegion = new(0, 0, size.Width, size.Height);
            TextSpriteAsset textSprite = new(texture, imageRegion);

            var newWeakRef = new WeakReference<TextSpriteAsset>(textSprite);
            _textSpriteCache[cacheKey] = (newWeakRef, texture);

            return textSprite;
        }
        finally
        {
            unsafe
            {
                SDL3.SDL_DestroySurface(surface);
            }
        }
    }

    public ShortSize MeasureTextSprite(string text, Font font)
    {
        int width = 0;
        int height = 0;
        unsafe
        {
            SDL3_ttf.TTF_GetStringSizeWrapped(font.TtfFont, text, 0, 0, &width, &height);
        }

        return new ShortSize((ushort)width, (ushort)height);
    }

    private Pointer<SDL_Surface> RenderTextToSurface(string text, Font font)
    {
        Color usedColor = Colors.White;
        unsafe
        {
            SDL_Color white = (SDL_Color)usedColor;
            // length is in bytes, length=0 means till '\0'
            // TODO: allow changing wrap_width
            Pointer<SDL_Surface> surface = font.RasterizationMode switch
            {
                FontRasterizationMode.Solid => SDL3_ttf.TTF_RenderText_Solid_Wrapped(font.TtfFont, text, 0, white, 0),
                FontRasterizationMode.Lcd => SDL3_ttf.TTF_RenderText_LCD_Wrapped(font.TtfFont, text, 0, white, default, 0),
                _ => SDL3_ttf.TTF_RenderText_Blended_Wrapped(font.TtfFont, text, 0, white, 0)
            };
            string renderFunctionName = font.RasterizationMode switch
            {
                FontRasterizationMode.Solid => nameof(SDL3_ttf.TTF_RenderText_Solid_Wrapped),
                FontRasterizationMode.Lcd => nameof(SDL3_ttf.TTF_RenderText_LCD_Wrapped),
                _ => nameof(SDL3_ttf.TTF_RenderText_Blended_Wrapped)
            };
            SdlError.ThrowOnNull(surface, renderFunctionName);

            return new Pointer<SDL_Surface>(surface);
        }
    }

    private static TTF_HintingFlags ToSdlHintingMode(FontHintingMode hintingMode)
    {
        return hintingMode switch
        {
            FontHintingMode.Light => TTF_HintingFlags.TTF_HINTING_LIGHT,
            FontHintingMode.Mono => TTF_HintingFlags.TTF_HINTING_MONO,
            FontHintingMode.None => TTF_HintingFlags.TTF_HINTING_NONE,
            FontHintingMode.LightSubpixel => TTF_HintingFlags.TTF_HINTING_LIGHT_SUBPIXEL,
            _ => TTF_HintingFlags.TTF_HINTING_NORMAL
        };
    }

    private Texture CreateTextureFromSurface(Pointer<SDL_Surface> surface)
    {
        unsafe
        {
            Pointer<SDL_Surface> convertedSurface = SDL3.SDL_ConvertSurface(surface, SDL_PixelFormat.SDL_PIXELFORMAT_ABGR8888);
            SdlError.ThrowOnNull(convertedSurface, nameof(SDL3.SDL_ConvertSurface));

            try
            {
                SDL_Surface* sdlSurface = convertedSurface;
                ShortSize size = new ShortSize((ushort)sdlSurface->w, (ushort)sdlSurface->h);
                int pitch = sdlSurface->pitch;
                byte* pixelData = (byte*)sdlSurface->pixels;
                int width = sdlSurface->w;
                int height = sdlSurface->h;
                byte[] data = new byte[width * height * 4];

                fixed (byte* dataPtr = data)
                {
                    byte* dst = dataPtr;
                    for (int y = 0; y < height; y++)
                    {
                        byte* src = pixelData + (y * pitch);
                        Buffer.MemoryCopy(src, dst, width * 4, width * 4);
                        dst += width * 4;
                    }
                }

                RawImage image = new RawImage(data, size, PixelFormat.Rgba8888);
                Texture texture = _gpuMemorySystem.CreateTexture(image);

                return texture;
            }
            finally
            {
                SDL3.SDL_DestroySurface(convertedSurface);
            }
        }
    }

    public void Update()
    {
        var keysToRemove = new List<(string text, Font font)>();
        foreach (var kvp in _textSpriteCache)
        {
            if (!kvp.Value.WeakRef.TryGetTarget(out _))
            {
                kvp.Value.Texture.Dispose();
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _textSpriteCache.Remove(key);
        }
    }

    public void ReleaseTextSprite(TextSpriteAsset textSprite)
    {
        var keysToRemove = new List<(string text, Font font)>();
        foreach (var kvp in _textSpriteCache)
        {
            if (kvp.Value.WeakRef.TryGetTarget(out TextSpriteAsset? target) && ReferenceEquals(target, textSprite))
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _textSpriteCache.Remove(key);
        }

        textSprite.Dispose();
    }

    public void ReleaseFont(Font font)
    {
        _fonts.Remove(font);
        _fontCache.Remove((font.Path, font.Size, font.RasterizationMode, font.HintingMode));

        unsafe
        {
            SDL3_ttf.TTF_CloseFont(font.TtfFont);
        }

        font.FreeFontData();
    }

    public void Dispose()
    {
        foreach (var kvp in _textSpriteCache)
        {
            kvp.Value.Texture.Dispose();
        }
        _textSpriteCache.Clear();

        List<Font> fonts = new(_fonts);
        foreach (Font font in fonts)
        {
            ReleaseFont(font);
        }

        SDL3_ttf.TTF_Quit();
    }
}
