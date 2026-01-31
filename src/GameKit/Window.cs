using GameKit.Common;
using GameKit.Gpu;
using GameKit.Utilities;
using SDL;

namespace GameKit;

internal class Window : IWindow
{
    internal Pointer<SDL_GPUDevice> SdlGpuDevice { get; }
    internal Pointer<SDL_Window> SdlWindow { get; private set; }
    
    public uint Id { get; }

    internal Window(Pointer<SDL_Window> sdlWindow, Pointer<SDL_GPUDevice> sdlSdlGpuDevice, uint id)
    {
        SdlGpuDevice = sdlSdlGpuDevice;
        SdlWindow = sdlWindow;
        Id = id;
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

    public bool TryAcquireSwapchainTexture(CommandBuffer commandBuffer, out SwapchainTexture swapchainTexture)
    {
        swapchainTexture = default!;
        uint width, height;

        unsafe
        {
            SDL_GPUTexture* swapchainTexturePointer;
            if (SDL3.SDL_AcquireGPUSwapchainTexture(commandBuffer.SdlGpuCommandBuffer, SdlWindow, &swapchainTexturePointer, &width, &height) == false)
            {
                throw new GameKitInitializationException($"SDL_AcquireGPUSwapchainTexture failed: {SDL3.SDL_GetError()}");
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
}