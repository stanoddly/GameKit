using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;
using SDL;

namespace GameKit.Text;

internal class FontSystem: IFontSystem, IUpdatable
{
    private readonly GpuMemorySystem _gpuMemorySystem;
    private readonly VirtualFileSystem _fileSystem;
    private readonly List<Font> _fonts = new();
    private readonly Dictionary<(string path, ushort size), Font> _fontCache = new();
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

    public Font Load(string path, ushort size)
    {
        var cacheKey = (path, size);
        if (_fontCache.TryGetValue(cacheKey, out Font? cachedFont))
        {
            return cachedFont;
        }

        VirtualFile fontFile = _fileSystem.GetFile(path);

        using Stream stream = fontFile.Open();
        byte[] fontData = new byte[stream.Length];
        stream.ReadExactly(fontData);

        unsafe
        {
            fixed (byte* fontDataPtr = fontData)
            {
                Pointer<SDL_IOStream> sdlStream = SDL3.SDL_IOFromConstMem((IntPtr)fontDataPtr, (UIntPtr)stream.Length);
                SdlError.ThrowOnNull(sdlStream, nameof(SDL3.SDL_IOFromConstMem));

                Pointer<TTF_Font> ttfFont = SDL3_ttf.TTF_OpenFontIO(sdlStream, true, size);
                SdlError.ThrowOnNull(ttfFont, nameof(SDL3_ttf.TTF_OpenFontIO));

                Font font = new Font(this, ttfFont, path, size);
                _fonts.Add(font);
                _fontCache[cacheKey] = font;

                return font;
            }
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
            ShortRectangle imageRegion = new(0, 0, (short)size.Width, (short)size.Height);
            TextSpriteAsset textSprite = new(cached.Texture, imageRegion);
            
            _textSpriteCache[cacheKey] = (new WeakReference<TextSpriteAsset>(textSprite), cached.Texture);
            return textSprite;
        }

        Pointer<SDL_Surface> surface = RenderTextToSurface(text, font);
        try
        {
            Texture texture = CreateTextureFromSurface(surface);
            ShortSize size = texture.Size;
            ShortRectangle imageRegion = new(0, 0, (short)size.Width, (short)size.Height);
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
            Pointer<SDL_Surface> surface = SDL3_ttf.TTF_RenderText_Blended_Wrapped(font.TtfFont, text, 0, white, 0);
            SdlError.ThrowOnNull(surface, nameof(SDL3_ttf.TTF_RenderText_Blended_Wrapped));

            return new Pointer<SDL_Surface>(surface);
        }
    }

    private Texture CreateTextureFromSurface(Pointer<SDL_Surface> surface)
    {
        unsafe
        {
            SDL_Surface* sdlSurface = surface;
            var size = new ShortSize((ushort)sdlSurface->w, (ushort)sdlSurface->h);
            var pitch = sdlSurface->pitch;
            var pixelData = (byte*)sdlSurface->pixels;
            var totalBytes = pitch * sdlSurface->h;
            
            var pixelFormat = (PixelFormat)sdlSurface->format;
            
            byte[] data;
            PixelFormat targetFormat;
            
            if (pixelFormat == PixelFormat.Argb8888)
            {
                int width = sdlSurface->w;
                int height = sdlSurface->h;
                data = new byte[width * height * 4];
                
                fixed (byte* dataPtr = data)
                {
                    byte* dst = dataPtr;
                    
                    for (int y = 0; y < height; y++)
                    {
                        byte* src = pixelData + (y * pitch);
                        
                        for (int x = 0; x < width; x++)
                        {
                            byte b = src[0];
                            byte g = src[1];
                            byte r = src[2];
                            byte a = src[3];
                            
                            dst[0] = r;
                            dst[1] = g;
                            dst[2] = b;
                            dst[3] = a;
                            
                            src += 4;
                            dst += 4;
                        }
                    }
                }
                
                targetFormat = PixelFormat.Rgba8888;
            }
            else
            {
                data = new byte[totalBytes];
                fixed (byte* dataPtr = data)
                {
                    Buffer.MemoryCopy(pixelData, dataPtr, totalBytes, totalBytes);
                }
                targetFormat = pixelFormat;
            }
            
            var image = new RawImage(data, size, targetFormat);
            
            Texture texture = _gpuMemorySystem.CreateTexture(image);

            return texture;
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
        _fontCache.Remove((font.Path, font.Size));

        unsafe
        {
            SDL3_ttf.TTF_CloseFont(font.TtfFont);
        }
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
