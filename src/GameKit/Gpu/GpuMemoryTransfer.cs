using System.Runtime.CompilerServices;
using GameKit.Content;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public struct GpuMemoryTransfer: IDisposable
{
    private readonly Pointer<SDL_GPUDevice> _sdlGpuDevice;

    private Pointer<SDL_GPUCopyPass> _copyPass;
    private Pointer<SDL_GPUCommandBuffer> _uploadCommandBuffer;
    
    List<Pointer<SDL_GPUTransferBuffer>> _tranferBuffers = new();

    public GpuMemoryTransfer(Pointer<SDL_GPUDevice> sdlGpuDevice, Pointer<SDL_GPUCommandBuffer> uploadCommandBuffer, Pointer<SDL_GPUCopyPass> copyPass)
    {
        _sdlGpuDevice = sdlGpuDevice;
        _copyPass = copyPass;
        _uploadCommandBuffer = uploadCommandBuffer;
    }

    public VertexBuffer<TVertexType> AddVertexBuffer<TVertexType>(ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType
    {
        uint sizeBytes = (uint)(Unsafe.SizeOf<TVertexType>() * vertices.Length);
        unsafe
        {
            SDL_GPUBufferCreateInfo sdlGpuBufferCreateInfo = new SDL_GPUBufferCreateInfo()
            {
                usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX,
                size = sizeBytes
            };
            
            SDL_GPUBuffer* vertexBuffer = SDL3.SDL_CreateGPUBuffer(_sdlGpuDevice, &sdlGpuBufferCreateInfo);
            
            SDL_GPUTransferBufferCreateInfo sdlGpuTransferBufferCreateInfo = new SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = sizeBytes
            };
            
            SDL_GPUTransferBuffer* transferBuffer = SDL3.SDL_CreateGPUTransferBuffer(_sdlGpuDevice, &sdlGpuTransferBufferCreateInfo);
            _tranferBuffers.Add(transferBuffer);

            TVertexType* transferBufferPointer = (TVertexType*)SDL3.SDL_MapGPUTransferBuffer(_sdlGpuDevice, transferBuffer, false);
            Span<TVertexType> transferBufferSpan = new Span<TVertexType>(transferBufferPointer, vertices.Length);
            
            vertices.CopyTo(transferBufferSpan);
            
            SDL3.SDL_UnmapGPUTransferBuffer(_sdlGpuDevice, transferBuffer);
            
            SDL_GPUTransferBufferLocation sdlGpuTransferBufferLocation = new SDL_GPUTransferBufferLocation { transfer_buffer = transferBuffer, offset = 0 };
            SDL_GPUBufferRegion sdlGpuBufferRegion = new SDL_GPUBufferRegion
                { buffer = vertexBuffer, offset = 0, size = sizeBytes };
            
            SDL3.SDL_UploadToGPUBuffer(_copyPass, &sdlGpuTransferBufferLocation, &sdlGpuBufferRegion, false);
            
            return new VertexBuffer<TVertexType>(vertexBuffer, vertices.Length);
        }
    }

    public VertexBuffer<TVertexType> AddVertexBuffer<TVertexType>(in Shape<TVertexType> shape)
        where TVertexType : unmanaged, IVertexType
    {
        return AddVertexBuffer((ReadOnlySpan<TVertexType>)shape);
    }

    public Texture AddTexture(Image image)
    {
        SdlError.Clear();

        // TODO: check parameters
        ReadOnlySpan<byte> imageData = image.Data;
        (int width, int height) = image.Size;
        uint sizeInBytes = (uint)imageData.Length;

        unsafe
        {
            SDL_GPUTextureCreateInfo sdlGpuTextureCreateInfo = new SDL_GPUTextureCreateInfo
            {
                type = SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
                format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_R8G8B8A8_UNORM,
                width = (uint)width,
                height = (uint)height,
                layer_count_or_depth = 1,
                num_levels = 1,
                usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER
            };
            Pointer<SDL_GPUTexture> sdlGpuTexture = SDL3.SDL_CreateGPUTexture(_sdlGpuDevice, &sdlGpuTextureCreateInfo);

            SDL_GPUTransferBufferCreateInfo sdlGpuTransferBufferCreateInfo = new SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = (uint)(width * height * 4)
            };
            SDL_GPUTransferBuffer* textureTransferBuffer = SDL3.SDL_CreateGPUTransferBuffer(
                _sdlGpuDevice,
                &sdlGpuTransferBufferCreateInfo
            );

            ushort* textureTransfer = (ushort*)SDL3.SDL_MapGPUTransferBuffer(_sdlGpuDevice, textureTransferBuffer, false);
            fixed (byte* textureDataPointer = imageData)
            {
                Buffer.MemoryCopy(textureDataPointer, textureTransfer, sizeInBytes, sizeInBytes);
            }
            
            SDL3.SDL_UnmapGPUTransferBuffer(_sdlGpuDevice, textureTransferBuffer);

            SDL_GPUTextureTransferInfo sdlGpuTextureTransferInfo = new SDL_GPUTextureTransferInfo
            {
                transfer_buffer = textureTransferBuffer,
                offset = 0
            };

            SDL_GPUTextureRegion sdlGpuTextureRegion = new SDL_GPUTextureRegion
            {
                texture = sdlGpuTexture,
                w = (uint)width,
                h = (uint)height,
                d = 1
            };

            SDL3.SDL_UploadToGPUTexture(
                _copyPass,
                &sdlGpuTextureTransferInfo,
                &sdlGpuTextureRegion,
                false);
            SdlError.ThrowOnError();

            return new Texture(sdlGpuTexture, width, height);
        }
    }

    public void Dispose()
    {
        // TODO: exception on _uploadCommandBuffer == null
        unsafe
        {
            SDL3.SDL_EndGPUCopyPass(_copyPass);
            _copyPass = null;
            SDL3.SDL_SubmitGPUCommandBuffer(_uploadCommandBuffer);
            _uploadCommandBuffer = null;

            foreach (Pointer<SDL_GPUTransferBuffer> tranferBuffer in _tranferBuffers)
            {
                SDL3.SDL_ReleaseGPUTransferBuffer(_sdlGpuDevice, tranferBuffer);
            }
            _tranferBuffers.Clear();
        }
    }
}