using GameKit.Gpu;
using GameKit.Utilities;
using SDL;

namespace GameKit;

public sealed class ActivationWindow : IDisposable
{
    internal Pointer<SDL_GPUDevice> SdlGpuDevice { get; }
    internal Pointer<SDL_Window> SdlWindow { get; private set; }

    public uint Id { get; }

    internal ActivationWindow(
        Pointer<SDL_Window> sdlWindow,
        Pointer<SDL_GPUDevice> sdlGpuDevice,
        uint id)
    {
        SdlWindow = sdlWindow;
        SdlGpuDevice = sdlGpuDevice;
        Id = id;
    }

    public ShortSize RenderSizeInPixels
    {
        get
        {
            int width;
            int height;
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

    public bool TryWaitAndAcquireSwapchainTexture(
        CommandBuffer commandBuffer,
        out SwapchainTexture swapchainTexture)
    {
        swapchainTexture = default!;
        uint width;
        uint height;

        unsafe
        {
            SDL_GPUTexture* swapchainTexturePointer;
            if (!SDL3.SDL_WaitAndAcquireGPUSwapchainTexture(
                    commandBuffer.SdlGpuCommandBuffer,
                    SdlWindow,
                    &swapchainTexturePointer,
                    &width,
                    &height))
            {
                throw new GameKitException(
                    $"SDL_WaitAndAcquireGPUSwapchainTexture failed: {SDL3.SDL_GetError()}");
            }

            if (swapchainTexturePointer == null)
            {
                return false;
            }

            TextureFormat textureFormat =
                (TextureFormat)SDL3.SDL_GetGPUSwapchainTextureFormat(SdlGpuDevice, SdlWindow);
            swapchainTexture = new SwapchainTexture(
                swapchainTexturePointer,
                new ShortSize((ushort)width, (ushort)height),
                textureFormat);
        }

        return true;
    }

    public void Dispose()
    {
        if (SdlWindow.IsNull)
        {
            return;
        }

        unsafe
        {
            SDL3.SDL_ReleaseWindowFromGPUDevice(SdlGpuDevice, SdlWindow);
            SDL3.SDL_DestroyWindow(SdlWindow);
            SdlWindow = Pointer<SDL_Window>.Null;
        }
    }
}
