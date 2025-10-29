using System.Runtime.CompilerServices;
using GameKit.Content;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class CopyPass: ICopyPass
{
    private Pointer<SDL_GPUCopyPass> _sdlCopyPass;
    private readonly GpuDevice _gpuDevice;
    private List<Pointer<SDL_GPUTransferBuffer>>? _transferBuffers;

    internal CopyPass(GpuDevice gpuDevice, Pointer<SDL_GPUCopyPass> sdlCopyPass)
    {
        _sdlCopyPass = sdlCopyPass;
        _gpuDevice = gpuDevice;
    }

    private unsafe SDL_GPUTransferBuffer* CreateAndTrackTransferBuffer(uint sizeBytes)
    {
        SDL_GPUTransferBufferCreateInfo sdlGpuTransferBufferCreateInfo = new SDL_GPUTransferBufferCreateInfo
        {
            usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
            size = sizeBytes
        };
        SDL_GPUTransferBuffer* transferBuffer = SDL3.SDL_CreateGPUTransferBuffer(_gpuDevice.SdlGpuDevice, &sdlGpuTransferBufferCreateInfo);
        SdlError.ThrowOnError();

        _transferBuffers ??= new();
        _transferBuffers.Add(transferBuffer);
        return transferBuffer;
    }

    public GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType
    {
        uint sizeBytes = (uint)(Unsafe.SizeOf<TVertexType>() * vertices.Length);
        unsafe
        {
            SDL_GPUBufferCreateInfo sdlGpuBufferCreateInfo = new SDL_GPUBufferCreateInfo()
            {
                usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX,
                size = sizeBytes
            };
            
            SDL_GPUBuffer* rawVertexBuffer = SDL3.SDL_CreateGPUBuffer(_gpuDevice.SdlGpuDevice, &sdlGpuBufferCreateInfo);
            
            SDL_GPUTransferBuffer* transferBuffer = CreateAndTrackTransferBuffer(sizeBytes);

            TVertexType* transferBufferPointer = (TVertexType*)SDL3.SDL_MapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, transferBuffer, false);
            Span<TVertexType> transferBufferSpan = new Span<TVertexType>(transferBufferPointer, vertices.Length);
            
            vertices.CopyTo(transferBufferSpan);
            
            SDL3.SDL_UnmapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, transferBuffer);
            
            SDL_GPUTransferBufferLocation sdlGpuTransferBufferLocation = new SDL_GPUTransferBufferLocation { transfer_buffer = transferBuffer, offset = 0 };
            SDL_GPUBufferRegion sdlGpuBufferRegion = new SDL_GPUBufferRegion
                { buffer = rawVertexBuffer, offset = 0, size = sizeBytes };
            
            SDL3.SDL_UploadToGPUBuffer(_sdlCopyPass, &sdlGpuTransferBufferLocation, &sdlGpuBufferRegion, false);
            
            GpuVertexBuffer<TVertexType> vertexBuffer = new GpuVertexBuffer<TVertexType>(_gpuDevice, rawVertexBuffer, Pointer<SDL_GPUBuffer>.Null, vertices.Length);
            _gpuDevice.RegisterVertexBuffer(vertexBuffer);
            return vertexBuffer;
        }
    }

    public GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(Shape<TVertexType> shape)
        where TVertexType : unmanaged, IVertexType
    {
        return CreateVertexBuffer((ReadOnlySpan<TVertexType>)shape);
    }
    
    public void UpdateVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> vertexBuffer, ReadOnlySpan<TVertexType> vertices) where TVertexType: unmanaged, IVertexType
    {
        uint sizeBytes = (uint)(Unsafe.SizeOf<TVertexType>() * vertices.Length);

        if (sizeBytes == 0)
        {
            throw new ArgumentException($"{nameof(vertices.Length)} is 0");
        }
        
        uint bufferSizeBytes = (uint)vertexBuffer.BufferSizeBytes;

        if (sizeBytes > bufferSizeBytes)
        {
            throw new ArgumentException($"{nameof(vertices)} cannot fit to {nameof(vertexBuffer)}");
        }

        unsafe
        {
            SDL_GPUTransferBuffer* transferBuffer = CreateAndTrackTransferBuffer(sizeBytes);

            TVertexType* transferBufferPointer = (TVertexType*)SDL3.SDL_MapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, transferBuffer, false);
            Span<TVertexType> transferBufferSpan = new Span<TVertexType>(transferBufferPointer, vertices.Length);
            
            vertices.CopyTo(transferBufferSpan);
            
            SDL3.SDL_UnmapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, transferBuffer);
            
            SDL_GPUTransferBufferLocation sdlGpuTransferBufferLocation = new SDL_GPUTransferBufferLocation { transfer_buffer = transferBuffer, offset = 0 };
            SDL_GPUBufferRegion sdlGpuBufferRegion = new SDL_GPUBufferRegion
                { buffer = vertexBuffer.SdlVertexBuffer, offset = 0, size = sizeBytes };
            
            SDL3.SDL_UploadToGPUBuffer(_sdlCopyPass, &sdlGpuTransferBufferLocation, &sdlGpuBufferRegion, false);
        }
        
        vertexBuffer.Size = vertices.Length;
    }

    public Texture CreateTexture(Image image)
    {
        SdlError.Clear();

        // TODO: check parameters
        ReadOnlySpan<byte> imageData = image.Data;
        (ushort width, ushort height) = image.Size;
        uint sizeInBytes = (uint)imageData.Length;

        SdlError.ThrowOnError();
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
                usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER,
                // TODO: this is actually SDL bug
                props = SDL3.SDL_CreateProperties()
            };
            Pointer<SDL_GPUTexture> sdlGpuTexture = SDL3.SDL_CreateGPUTexture(_gpuDevice.SdlGpuDevice, &sdlGpuTextureCreateInfo);
            SdlError.ThrowOnError();

            SDL_GPUTransferBuffer* textureTransferBuffer = CreateAndTrackTransferBuffer((uint)(width * height * 4));

            ushort* textureTransfer = (ushort*)SDL3.SDL_MapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, textureTransferBuffer, false);
            fixed (byte* textureDataPointer = imageData)
            {
                Buffer.MemoryCopy(textureDataPointer, textureTransfer, sizeInBytes, sizeInBytes);
            }
            
            SDL3.SDL_UnmapGPUTransferBuffer(_gpuDevice.SdlGpuDevice, textureTransferBuffer);
            SdlError.ThrowOnError();

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
                _sdlCopyPass,
                &sdlGpuTextureTransferInfo,
                &sdlGpuTextureRegion,
                false);
            SdlError.ThrowOnError();

            Texture texture = new UserTexture(_gpuDevice, sdlGpuTexture, (width, height), TextureFormat.R8G8B8A8Unorm);
            _gpuDevice.RegisterTexture(texture);
            return texture;
        }
    }

    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_EndGPUCopyPass(_sdlCopyPass);
            _sdlCopyPass = null;

            if (_transferBuffers == null)
            {
                return;
            }

            foreach (Pointer<SDL_GPUTransferBuffer> transferBuffer in _transferBuffers)
            {
                SDL3.SDL_ReleaseGPUTransferBuffer(_gpuDevice.SdlGpuDevice, transferBuffer);
            }
            _transferBuffers.Clear();
        }
    }
}
