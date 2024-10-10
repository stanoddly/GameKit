using System.Runtime.CompilerServices;
using SDL;

namespace GameKit;

public struct VertexBuffer<TVertexType> where TVertexType : unmanaged, IVertexType
{
    public Pointer<SDL_GPUBuffer> SdlVertexBuffer { get; private set; }
    public Pointer<SDL_GPUBuffer>? SdlIndexBuffer { get; private set; }
    public int Size { get; private set; }

    public VertexBuffer(Pointer<SDL_GPUBuffer> sdlVertexBuffer, int size)
    {
        SdlVertexBuffer = sdlVertexBuffer;
        Size = size;
    }
}



public struct GpuDevice: IDisposable
{
    private static readonly (float r, float g, float b, float a) DefaultClearColor = (0.3f, 0.4f, 0.5f, 1.0f);

    public Pointer<SDL_GPUDevice> SdlGpuDevice { get; private set; }
    public Pointer<SDL_Window> SdlWindow { get; private set; }
    
    public GpuDevice(Pointer<SDL_GPUDevice> sdlGpuDevice, Pointer<SDL_Window> sdlWindow)
    {
        SdlGpuDevice = sdlGpuDevice;
        SdlWindow = sdlWindow;
    }

    public ShaderFormats GetSupportedShaderFormats()
    {
        unsafe
        {
            SDL_GPUShaderFormat formats = SDL3.SDL_GetGPUShaderFormats(SdlGpuDevice);
            
            ShaderFormats shaderFormats = new ShaderFormats((uint)formats);

            return shaderFormats;
        }
    }

    public GpuCommandBuffer AcquireCommandBuffer()
    {
        unsafe
        {
            Pointer<SDL_GPUCommandBuffer> sdlGpuCommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(SdlGpuDevice);
            
            if (sdlGpuCommandBuffer.IsNull())
            {
                throw new GameKitInitializationException($"SDL_AcquireGPUCommandBuffer failed: {SDL3.SDL_GetError()}");
            }

            return new GpuCommandBuffer(sdlGpuCommandBuffer);
        }
    }

    public VertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(Span<TVertexType> vertices) where TVertexType : unmanaged, IVertexType
    {
        SdlError.Clear();
        uint sizeBytes = (uint)(Unsafe.SizeOf<TVertexType>() * vertices.Length);
        unsafe
        {
            SDL_GPUBufferCreateInfo sdlGpuBufferCreateInfo = new SDL_GPUBufferCreateInfo()
            {
                usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX,
                size = sizeBytes
            };
            
            // Create the vertex buffer
            SDL_GPUBuffer* vertexBuffer = SDL3.SDL_CreateGPUBuffer(SdlGpuDevice, &sdlGpuBufferCreateInfo);
            SdlError.ThrowOnNull(vertexBuffer);

            SDL_GPUTransferBufferCreateInfo sdlGpuTransferBufferCreateInfo = new SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = sizeBytes
            };

            // To get data into the vertex buffer, we have to use a transfer buffer
            SDL_GPUTransferBuffer* transferBuffer = SDL3.SDL_CreateGPUTransferBuffer(SdlGpuDevice, &sdlGpuTransferBufferCreateInfo);
            SdlError.ThrowOnNull(transferBuffer);

            TVertexType* transferBufferPointer = (TVertexType*)SDL3.SDL_MapGPUTransferBuffer(SdlGpuDevice, transferBuffer, SDL_bool.SDL_FALSE);
            SdlError.ThrowOnNull(transferBufferPointer);
            Span<TVertexType> transferBufferSpan = new Span<TVertexType>(transferBufferPointer, vertices.Length);
            
            // TODO: provide a way to copy data directly into a transfer buffer
            vertices.CopyTo(transferBufferSpan);
            
            SDL3.SDL_UnmapGPUTransferBuffer(SdlGpuDevice, transferBuffer);
            SdlError.ThrowOnError();

            SDL_GPUCommandBuffer* uploadCmdBuf = SDL3.SDL_AcquireGPUCommandBuffer(SdlGpuDevice);
            SdlError.ThrowOnNull(uploadCmdBuf);

            SDL_GPUCopyPass* copyPass = SDL3.SDL_BeginGPUCopyPass(uploadCmdBuf);
            SdlError.ThrowOnNull(copyPass);
            
            SDL_GPUTransferBufferLocation sdlGpuTransferBufferLocation = new SDL_GPUTransferBufferLocation { transfer_buffer = transferBuffer, offset = 0 };
            SDL_GPUBufferRegion sdlGpuBufferRegion = new SDL_GPUBufferRegion
                { buffer = vertexBuffer, offset = 0, size = sizeBytes };
            SDL3.SDL_UploadToGPUBuffer(copyPass, &sdlGpuTransferBufferLocation, &sdlGpuBufferRegion, SDL_bool.SDL_FALSE);
            SdlError.ThrowOnError();
            SDL3.SDL_EndGPUCopyPass(copyPass);
            SdlError.ThrowOnError();
            SDL3.SDL_SubmitGPUCommandBuffer(uploadCmdBuf);
            SdlError.ThrowOnError();
            SDL3.SDL_ReleaseGPUTransferBuffer(SdlGpuDevice, transferBuffer);
            SdlError.ThrowOnError();

            return new VertexBuffer<TVertexType>(vertexBuffer, vertices.Length);
        }
    }

    public FrameRenderContext CreateFrameRenderContext()
    {
        return CreateFrameRenderContext(DefaultClearColor);
    }

    public FrameRenderContext CreateFrameRenderContext((float r, float g, float b, float a) clearColor)
    {
        unsafe
        {
            GpuCommandBuffer gpuCommandBuffer = AcquireCommandBuffer();

            uint width, height;
            SDL_GPUTexture* swapchainTexturePointer = SDL3.SDL_AcquireGPUSwapchainTexture(gpuCommandBuffer.Pointer, SdlWindow, &width, &height);
            if (swapchainTexturePointer == null)
            {
                throw new GameKitInitializationException($"SDL_AcquireGPUSwapchainTexture failed: {SDL3.SDL_GetError()}");
            }

            Texture swapchainTexture = new Texture(swapchainTexturePointer, (int)width, (int)height);

            SDL_GPUColorTargetInfo colorTargetInfo = new SDL_GPUColorTargetInfo();
            colorTargetInfo.texture = swapchainTexturePointer;
            colorTargetInfo.clear_color = new SDL_FColor { r = clearColor.r, g = clearColor.g, b = clearColor.b, a = clearColor.a };
            colorTargetInfo.load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR;
            colorTargetInfo.store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE;

            return new FrameRenderContext {ColorTargetInfo = colorTargetInfo, GpuCommandBuffer = gpuCommandBuffer, Size = (width, height), SwapchainTexture = swapchainTexture };
        }
    }

    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_DestroyGPUDevice(SdlGpuDevice);
            SdlGpuDevice = null;
        }
    }
}
