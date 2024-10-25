using System.Runtime.CompilerServices;
using SDL;

namespace GameKit;

public struct MemoryTransfer: IDisposable
{
    private readonly GpuDevice _gpuDevice;

    private Pointer<SDL_GPUCopyPass> _copyPass;
    private Pointer<SDL_GPUCommandBuffer> _uploadCommandBuffer;
    
    List<Pointer<SDL_GPUTransferBuffer>> _tranferBuffers = new();

    public MemoryTransfer(GpuDevice gpuDevice, Pointer<SDL_GPUCommandBuffer> uploadCommandBuffer, Pointer<SDL_GPUCopyPass> copyPass)
    {
        _gpuDevice = gpuDevice;
        _copyPass = copyPass;
        _uploadCommandBuffer = uploadCommandBuffer;
    }

    public VertexBuffer<TVertexType> AddVertexBuffer<TVertexType>(Span<TVertexType> vertices) where TVertexType: unmanaged, IVertexType 
    {
        uint sizeBytes = (uint)(Unsafe.SizeOf<TVertexType>() * vertices.Length);
        unsafe
        {
            SDL_GPUBufferCreateInfo sdlGpuBufferCreateInfo = new SDL_GPUBufferCreateInfo()
            {
                usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX,
                size = sizeBytes
            };
            
            SDL_GPUBuffer* vertexBuffer = SDL3.SDL_CreateGPUBuffer(_gpuDevice.SdlGpuDevice, &sdlGpuBufferCreateInfo);
            
            SDL_GPUTransferBufferCreateInfo sdlGpuTransferBufferCreateInfo = new SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = sizeBytes
            };
            
            SDL_GPUTransferBuffer* transferBuffer = SDL3.SDL_CreateGPUTransferBuffer(_gpuDevice.SdlGpuDevice, &sdlGpuTransferBufferCreateInfo);
            _tranferBuffers.Add(transferBuffer);

            TVertexType* transferBufferPointer = (TVertexType*)SDL3.SDL_MapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, transferBuffer, SDL_bool.SDL_FALSE);
            Span<TVertexType> transferBufferSpan = new Span<TVertexType>(transferBufferPointer, vertices.Length);
            
            vertices.CopyTo(transferBufferSpan);
            
            SDL3.SDL_UnmapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, transferBuffer);
            
            SDL_GPUTransferBufferLocation sdlGpuTransferBufferLocation = new SDL_GPUTransferBufferLocation { transfer_buffer = transferBuffer, offset = 0 };
            SDL_GPUBufferRegion sdlGpuBufferRegion = new SDL_GPUBufferRegion
                { buffer = vertexBuffer, offset = 0, size = sizeBytes };
            
            SDL3.SDL_UploadToGPUBuffer(_copyPass, &sdlGpuTransferBufferLocation, &sdlGpuBufferRegion, SDL_bool.SDL_FALSE);
            
            return new VertexBuffer<TVertexType>(vertexBuffer, vertices.Length);
        }
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
            Pointer<SDL_GPUTexture> sdlGpuTexture = SDL3.SDL_CreateGPUTexture(_gpuDevice.SdlGpuDevice, &sdlGpuTextureCreateInfo);

            SDL_GPUTransferBufferCreateInfo sdlGpuTransferBufferCreateInfo = new SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = (uint)(width * height * 4)
            };
            SDL_GPUTransferBuffer* textureTransferBuffer = SDL3.SDL_CreateGPUTransferBuffer(
                _gpuDevice.SdlGpuDevice,
                &sdlGpuTransferBufferCreateInfo
            );

            ushort* textureTransfer = (ushort*)SDL3.SDL_MapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, textureTransferBuffer, SDL_bool.SDL_FALSE);
            fixed (byte* textureDataPointer = imageData)
            {
                Buffer.MemoryCopy(textureDataPointer, textureTransfer, sizeInBytes, sizeInBytes);
            }
            
            SDL3.SDL_UnmapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, textureTransferBuffer);

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
                SDL_bool.SDL_FALSE);
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
                SDL3.SDL_ReleaseGPUTransferBuffer(_gpuDevice.SdlGpuDevice, tranferBuffer);
            }
            _tranferBuffers.Clear();
        }
    }
}

public class GpuMemoryUploader
{
    private GpuDevice _gpuDevice;

    public GpuMemoryUploader(GpuDevice gpuDevice)
    {
        _gpuDevice = gpuDevice;
    }

    public MemoryTransfer CreateMemoryTransfer()
    {
        unsafe
        {
            SDL_GPUCommandBuffer* uploadCmdBuf = SDL3.SDL_AcquireGPUCommandBuffer(_gpuDevice.SdlGpuDevice);
            SdlError.ThrowOnNull(uploadCmdBuf);

            SDL_GPUCopyPass* copyPass = SDL3.SDL_BeginGPUCopyPass(uploadCmdBuf);
            
            return new MemoryTransfer(_gpuDevice, uploadCmdBuf, copyPass);
        }
    }
}