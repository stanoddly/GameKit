using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GameKit.Common;
using GameKit.Content;
using GameKit.Gpu;
using GameKit.Utilities;
using SDL;

namespace GameKit;

internal class Window : IWindow
{
    internal Pointer<SDL_GPUDevice> SdlGpuDevice { get; }
    internal Pointer<SDL_Window> SdlWindow { get; private set; }
    private readonly GameKitFrameContext _frameContext;
    
    public uint Id { get; }

    private ShortSize _lastSize;

    public event ResolutionChangedHandler? ResolutionChanged;

    internal Window(Pointer<SDL_Window> sdlWindow, Pointer<SDL_GPUDevice> sdlSdlGpuDevice, uint id, GameKitFrameContext frameContext)
    {
        SdlGpuDevice = sdlSdlGpuDevice;
        SdlWindow = sdlWindow;
        Id = id;
        _frameContext = frameContext;
        _lastSize = RenderSizeInPixels;
    }

    internal void OnPixelSizeChanged(ulong timestamp)
    {
        ShortSize newSize = RenderSizeInPixels;
        ShortSize oldSize = _lastSize;

        if (newSize == oldSize) return;

        _lastSize = newSize;
        ResolutionChanged?.Invoke(new ResolutionChangedEventArgs(oldSize, newSize, timestamp));
    }

    public ShortSize RenderSizeInPixels
    {
        get
        {
            int width, height;
            unsafe
            {
                SDL3.SDL_GetWindowSizeInPixels(SdlWindow, &width, &height);
            }

            return new ShortSize((ushort)width, (ushort)height);
        }
    }

    public TextureFormat ColorTargetFormat
    {
        get
        {
            unsafe
            {
                return (TextureFormat)SDL3.SDL_GetGPUSwapchainTextureFormat(SdlGpuDevice, SdlWindow);
            }
        }
    }

    public bool MouseGrab
    {
        get
        {
            unsafe
            {
                return SDL3.SDL_GetWindowMouseGrab(SdlWindow);
            }
        }
        set
        {
            unsafe
            {
                SDL3.SDL_SetWindowMouseGrab(SdlWindow, value);
            }
        }
    }

    public bool WindowRelativeMouseMode
    {
        get
        {
            unsafe
            {
                return SDL3.SDL_GetWindowRelativeMouseMode(SdlWindow);
            }
        }
        set
        {
            unsafe
            {
                SDL3.SDL_SetWindowRelativeMouseMode(SdlWindow, value);
            }
        }
    }

    public bool SupportsAlwaysOnTop
    {
        get
        {
            string? videoDriver = GetCurrentVideoDriver();
            return !string.Equals(videoDriver, "wayland", StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool AlwaysOnTop
    {
        get
        {
            unsafe
            {
                return (SDL3.SDL_GetWindowFlags(SdlWindow) & SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP) != 0;
            }
        }
        set
        {
            unsafe
            {
                if (SDL3.SDL_SetWindowAlwaysOnTop(SdlWindow, value) == false)
                {
                    throw new GameKitException($"SDL_SetWindowAlwaysOnTop failed: {SDL3.SDL_GetError()}");
                }
            }
        }
    }

    private static unsafe string? GetCurrentVideoDriver()
    {
        byte* videoDriver = SDL3.Unsafe_SDL_GetCurrentVideoDriver();
        return Marshal.PtrToStringUTF8((IntPtr)videoDriver);
    }

    public bool TryWaitAndAcquireSwapchainTexture(CommandBuffer commandBuffer, out SwapchainTexture swapchainTexture)
    {
        swapchainTexture = default!;
        uint width, height;

        unsafe
        {
            SDL_GPUTexture* swapchainTexturePointer;
            if (SDL3.SDL_WaitAndAcquireGPUSwapchainTexture(commandBuffer.SdlGpuCommandBuffer, SdlWindow, &swapchainTexturePointer, &width, &height) == false)
            {
                throw new GameKitInitializationException($"SDL_WaitAndAcquireGPUSwapchainTexture failed: {SDL3.SDL_GetError()}");
            }

            if (swapchainTexturePointer == null)
            {
                return false;
            }

            TextureFormat textureFormat = (TextureFormat)SDL3.SDL_GetGPUSwapchainTextureFormat(SdlGpuDevice, SdlWindow);

            swapchainTexture = new SwapchainTexture(swapchainTexturePointer, new ShortSize((ushort)width, (ushort)height), textureFormat);
        }

        return true;
    }

    public void SetFullscreenBorderless(bool fullscreen)
    {
        unsafe
        {
            SDL3.SDL_SetWindowFullscreen(SdlWindow, fullscreen);
        }
    }

    public void SetIcon(Image icon)
    {
        ReadOnlySpan<byte> pixelData = icon.Data;
        int width = icon.Size.Width;
        int height = icon.Size.Height;
        int pitch = width * 4;

        unsafe
        {
            fixed (byte* pixels = pixelData)
            {
                Pointer<SDL_Surface> surface = SDL3.SDL_CreateSurfaceFrom(
                    width,
                    height,
                    (SDL_PixelFormat)icon.PixelFormat,
                    (IntPtr)pixels,
                    pitch);

                if (surface.IsNull)
                {
                    throw new GameKitException($"SDL_CreateSurfaceFrom failed: {SDL3.SDL_GetError()}");
                }

                try
                {
                    if (!SDL3.SDL_SetWindowIcon(SdlWindow, surface))
                    {
                        throw new GameKitException($"SDL_SetWindowIcon failed: {SDL3.SDL_GetError()}");
                    }
                }
                finally
                {
                    SDL3.SDL_DestroySurface(surface);
                }
            }
        }
    }

    public FileDialogResult ShowModalOpenFileDialog(IReadOnlyList<FileDialogFilter>? filters = null, string? defaultLocation = null, bool allowMany = false)
    {
        _frameContext.Pause();
        try
        {
            return ShowModalFileDialog(filters ?? Array.Empty<FileDialogFilter>(), defaultLocation, allowMany, FileDialogKind.Open);
        }
        finally
        {
            _frameContext.Resume();
        }
    }

    public FileDialogResult ShowModalSaveFileDialog(IReadOnlyList<FileDialogFilter>? filters = null, string? defaultLocation = null)
    {
        _frameContext.Pause();
        try
        {
            return ShowModalFileDialog(filters ?? Array.Empty<FileDialogFilter>(), defaultLocation, false, FileDialogKind.Save);
        }
        finally
        {
            _frameContext.Resume();
        }
    }

    private unsafe FileDialogResult ShowModalFileDialog(
        IReadOnlyList<FileDialogFilter> filters,
        string? defaultLocation,
        bool allowMany,
        FileDialogKind kind)
    {
        ArgumentNullException.ThrowIfNull(filters);

        if (!SDL3.SDL_IsMainThread())
        {
            throw new GameKitException("File dialogs must be shown from the main thread.");
        }

        ModalFileDialogState state = new();
        GCHandle stateHandle = GCHandle.Alloc(state);
        IntPtr userdata = GCHandle.ToIntPtr(stateHandle);
        NativeFileDialogFilters nativeFilters = new(filters);
        IntPtr defaultLocationPointer = defaultLocation == null
            ? IntPtr.Zero
            : Marshal.StringToCoTaskMemUTF8(defaultLocation);

        try
        {
            fixed (SDL_DialogFileFilter* filtersPointer = nativeFilters.Filters)
            {
                SDL_DialogFileFilter* actualFiltersPointer = nativeFilters.Filters.Length == 0 ? null : filtersPointer;

                if (kind == FileDialogKind.Open)
                {
                    SDL3.SDL_ShowOpenFileDialog(
                        &OnFileDialogCompleted,
                        userdata,
                        SdlWindow,
                        actualFiltersPointer,
                        nativeFilters.Filters.Length,
                        (byte*)defaultLocationPointer,
                        allowMany);
                }
                else
                {
                    SDL3.SDL_ShowSaveFileDialog(
                        &OnFileDialogCompleted,
                        userdata,
                        SdlWindow,
                        actualFiltersPointer,
                        nativeFilters.Filters.Length,
                        (byte*)defaultLocationPointer);
                }

                while (!state.IsCompleted)
                {
                    SDL3.SDL_PumpEvents();
                    SDL3.SDL_Delay(10);
                }
            }

            return state.GetResult();
        }
        finally
        {
            stateHandle.Free();
            nativeFilters.Dispose();

            if (defaultLocationPointer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(defaultLocationPointer);
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void OnFileDialogCompleted(IntPtr userdata, byte** fileList, int filter)
    {
        GCHandle stateHandle = GCHandle.FromIntPtr(userdata);
        ModalFileDialogState state = (ModalFileDialogState)stateHandle.Target!;
        state.Complete(CreateFileDialogResult(fileList));
    }

    private static unsafe FileDialogResult CreateFileDialogResult(byte** fileList)
    {
        if (fileList == null)
        {
            string? error = SDL3.SDL_GetError();
            return FileDialogResult.Failed(string.IsNullOrWhiteSpace(error) ? "File dialog failed." : error);
        }

        List<string> paths = new();
        for (int i = 0; fileList[i] != null; i++)
        {
            string? path = Marshal.PtrToStringUTF8((IntPtr)fileList[i]);
            if (path != null)
            {
                paths.Add(path);
            }
        }

        if (paths.Count == 0)
        {
            return FileDialogResult.Canceled();
        }

        return FileDialogResult.Accepted(paths);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_ReleaseWindowFromGPUDevice(SdlGpuDevice, SdlWindow);
            SDL3.SDL_DestroyWindow(SdlWindow);
            SdlWindow = null;
        }
    }

    private sealed class ModalFileDialogState
    {
        private readonly ManualResetEventSlim _completed = new();
        private readonly object _lock = new();
        private FileDialogResult _result = FileDialogResult.Failed("File dialog did not complete.");

        public bool IsCompleted
        {
            get
            {
                return _completed.IsSet;
            }
        }

        public void Complete(FileDialogResult result)
        {
            lock (_lock)
            {
                _result = result;
            }

            _completed.Set();
        }

        public FileDialogResult GetResult()
        {
            lock (_lock)
            {
                return _result;
            }
        }
    }

    private enum FileDialogKind
    {
        Open,
        Save
    }

    private sealed class NativeFileDialogFilters : IDisposable
    {
        private readonly List<IntPtr> _allocatedStrings = new();

        public SDL_DialogFileFilter[] Filters { get; }

        public unsafe NativeFileDialogFilters(IReadOnlyList<FileDialogFilter> filters)
        {
            ArgumentNullException.ThrowIfNull(filters);

            Filters = new SDL_DialogFileFilter[filters.Count];

            for (int i = 0; i < filters.Count; i++)
            {
                if (filters[i].Name == null)
                {
                    throw new ArgumentException("Filter name cannot be null.", nameof(filters));
                }

                if (filters[i].Pattern == null)
                {
                    throw new ArgumentException("Filter pattern cannot be null.", nameof(filters));
                }

                IntPtr namePointer = Marshal.StringToCoTaskMemUTF8(filters[i].Name);
                IntPtr patternPointer = Marshal.StringToCoTaskMemUTF8(filters[i].Pattern);
                _allocatedStrings.Add(namePointer);
                _allocatedStrings.Add(patternPointer);

                Filters[i] = new SDL_DialogFileFilter
                {
                    name = (byte*)namePointer,
                    pattern = (byte*)patternPointer
                };
            }
        }

        public void Dispose()
        {
            foreach (IntPtr allocatedString in _allocatedStrings)
            {
                Marshal.FreeCoTaskMem(allocatedString);
            }
        }
    }
}
