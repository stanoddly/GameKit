using System.Runtime.CompilerServices;
using GameKit.Common;
using GameKit.Shaders;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

internal class GpuDevice : IGpuDevice
{
    private MemoryTrackedSet<Texture> _textures = new();
    private MemoryTrackedSet<GpuVertexBuffer> _vertexBuffers = new();
    private MemoryTrackedSet<GpuStorageBuffer> _storageBuffers = new();
    private LockedSet<Sampler> _samplers = new();
    private LockedSet<GraphicsPipeline> _graphicsPipelines = new();
    private LockedSet<Shader> _shaders = new();

    internal Pointer<SDL_GPUDevice> SdlGpuDevice { get; private set; }

    public GpuMemoryStats MemoryStats
    {
        get
        {
            (int Count, long TotalBytes) textures = _textures.CountAndTotalBytes;
            (int Count, long TotalBytes) vertexBuffers = _vertexBuffers.CountAndTotalBytes;
            (int Count, long TotalBytes) storageBuffers = _storageBuffers.CountAndTotalBytes;
            return new GpuMemoryStats(
                textures.Count, textures.TotalBytes,
                vertexBuffers.Count, vertexBuffers.TotalBytes,
                storageBuffers.Count, storageBuffers.TotalBytes
            );
        }
    }

    internal GpuDevice(Pointer<SDL_GPUDevice> sdlGpuDevice)
    {
        SdlGpuDevice = sdlGpuDevice;
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

    public CommandBuffer AcquireCommandBuffer()
    {
        unsafe
        {
            Pointer<SDL_GPUCommandBuffer> sdlGpuCommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(SdlGpuDevice);
            
            if (sdlGpuCommandBuffer.IsNull())
            {
                throw new GameKitInitializationException($"SDL_AcquireGPUCommandBuffer failed: {SDL3.SDL_GetError()}");
            }

            return new CommandBuffer(this, sdlGpuCommandBuffer);
        }
    }

    public Sampler CreateSampler(SamplerConfig config)
    {
        SDL_GPUSamplerCreateInfo sdlGpuSamplerCreateInfo = new SDL_GPUSamplerCreateInfo()
        {
            min_filter = (SDL_GPUFilter)config.MinFilter,
            mag_filter = (SDL_GPUFilter)config.MagFilter,
            mipmap_mode = (SDL_GPUSamplerMipmapMode)config.MipmapMode,
            address_mode_u = (SDL_GPUSamplerAddressMode)config.AddressModeU,
            address_mode_v = (SDL_GPUSamplerAddressMode)config.AddressModeV,
            address_mode_w = (SDL_GPUSamplerAddressMode)config.AddressModeW,
            mip_lod_bias = config.MipLodBias,
            min_lod = config.MinLod,
            max_lod = config.MaxLod,
            max_anisotropy = config.MaxAnisotropy,
            enable_anisotropy = config.EnableAnisotropy,
            compare_op = (SDL_GPUCompareOp)config.CompareOp,
            enable_compare = config.EnableCompare
        };

        unsafe
        {
            Pointer<SDL_GPUSampler> samplerPointer = SDL3.SDL_CreateGPUSampler(SdlGpuDevice, &sdlGpuSamplerCreateInfo);
            SdlError.ThrowOnNull(samplerPointer);

            var sampler = new Sampler(this, samplerPointer);
            _samplers.Add(sampler);
            return sampler;
        }
    }

    public Texture CreateDepthBufferTexture(ShortSize size, DepthBufferFormat format, bool sampler=false)
    {
        SDL_GPUTextureUsageFlags usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_DEPTH_STENCIL_TARGET;

        if (sampler)
        {
            usage |= SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER;
        }

        unsafe
        {
            SDL_GPUTextureCreateInfo info = new SDL_GPUTextureCreateInfo
            {
                usage = usage,
                format = (SDL_GPUTextureFormat)format,
                width = size.Width,
                height = size.Height,
                layer_count_or_depth = 1,
                num_levels = 1,
                sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1
            };

            Pointer<SDL_GPUTexture> rawTexture = SDL3.SDL_CreateGPUTexture(SdlGpuDevice, &info);
            SdlError.ThrowOnNull(rawTexture);

            Texture texture = new UserTexture(this, rawTexture, size, (TextureFormat)format);
            _textures.Add(texture);

            return texture;
        }
    }

    public Texture CreateColorTargetTexture(ShortSize size, TextureFormat format)
    {
        unsafe
        {
            SDL_GPUTextureCreateInfo info = new SDL_GPUTextureCreateInfo
            {
                usage = SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_COLOR_TARGET | SDL_GPUTextureUsageFlags.SDL_GPU_TEXTUREUSAGE_SAMPLER,
                format = (SDL_GPUTextureFormat)format,
                width = size.Width,
                height = size.Height,
                layer_count_or_depth = 1,
                num_levels = 1,
                sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1
            };

            Pointer<SDL_GPUTexture> rawTexture = SDL3.SDL_CreateGPUTexture(SdlGpuDevice, &info);
            SdlError.ThrowOnNull(rawTexture);

            Texture texture = new UserTexture(this, rawTexture, size, format);
            _textures.Add(texture);

            return texture;
        }
    }

    public void RegisterTexture(Texture texture) => _textures.Add(texture);

    public void RegisterVertexBuffer(GpuVertexBuffer vertexBuffer) => _vertexBuffers.Add(vertexBuffer);

    public void RegisterGraphicsPipeline(GraphicsPipeline graphicsPipeline) => _graphicsPipelines.Add(graphicsPipeline);

    public void RegisterShader(Shader shader) => _shaders.Add(shader);

    public void ReleaseTexture(Texture texture)
    {
        _textures.Remove(texture);
        Pointer<SDL_GPUTexture> pointer = texture.SdlGpuTexture;
        if (pointer.IsNull())
        {
            return;
        }

        unsafe
        {
            SDL3.SDL_ReleaseGPUTexture(SdlGpuDevice, texture.SdlGpuTexture);
        }

        texture.SdlGpuTexture = Pointer<SDL_GPUTexture>.Null;
    }
    
    public void ReleaseGraphicsPipeline(GraphicsPipeline pipeline)
    {
        _graphicsPipelines.Remove(pipeline);
        Pointer<SDL_GPUGraphicsPipeline> pointer = pipeline.Pointer;
        if (pointer.IsNull())
        {
            return;
        }

        unsafe
        {
            SDL3.SDL_ReleaseGPUGraphicsPipeline(SdlGpuDevice, pointer);
        }

        pipeline.Pointer = default;
    }

    public void ReleaseShader(Shader shader)
    {
        _shaders.Remove(shader);

        unsafe
        {
            SDL3.SDL_ReleaseGPUShader(SdlGpuDevice, shader.Pointer);
        }

        shader.Pointer = default;
    }

    public void ReleaseVertexBuffer(GpuVertexBuffer vertexBuffer)
    {
        _vertexBuffers.Remove(vertexBuffer);
        if (!vertexBuffer.SdlVertexBuffer.IsNull())
        {
            unsafe
            {
                SDL3.SDL_ReleaseGPUBuffer(SdlGpuDevice, vertexBuffer.SdlVertexBuffer);
            }

            vertexBuffer.SdlVertexBuffer = default;
        }

        if (!vertexBuffer.SdlIndexBuffer.IsNull())
        {
            unsafe
            {
                SDL3.SDL_ReleaseGPUBuffer(SdlGpuDevice, vertexBuffer.SdlIndexBuffer);
            }

            vertexBuffer.SdlIndexBuffer = default;
        }
    }

    public void RegisterStorageBuffer(GpuStorageBuffer storageBuffer) => _storageBuffers.Add(storageBuffer);

    public void ReleaseStorageBuffer(GpuStorageBuffer storageBuffer)
    {
        _storageBuffers.Remove(storageBuffer);
        if (!storageBuffer.SdlBuffer.IsNull())
        {
            unsafe
            {
                SDL3.SDL_ReleaseGPUBuffer(SdlGpuDevice, storageBuffer.SdlBuffer);
            }

            storageBuffer.SdlBuffer = default;
        }
    }

    public void ReleaseSampler(Sampler sampler)
    {
        _samplers.Remove(sampler);

        unsafe
        {
            SDL3.SDL_ReleaseGPUSampler(SdlGpuDevice, sampler.Pointer);
        }

        sampler.Pointer = default;
    }

    public GpuVertexBuffer<TVertexType> CreateVertexBuffer<TVertexType>(int length) where TVertexType: unmanaged, IVertexType
    {
        uint sizeBytes = (uint)(Unsafe.SizeOf<TVertexType>() * length);
        unsafe
        {
            SDL_GPUBufferCreateInfo sdlGpuBufferCreateInfo = new SDL_GPUBufferCreateInfo()
            {
                usage = SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX,
                size = sizeBytes
            };

            SDL_GPUBuffer* rawVertexBuffer = SDL3.SDL_CreateGPUBuffer(SdlGpuDevice, &sdlGpuBufferCreateInfo);

            return new GpuVertexBuffer<TVertexType>(this, rawVertexBuffer, Pointer<SDL_GPUBuffer>.Null, length);
        }
    }

    public void WaitForFences(ReadOnlySpan<GpuFence> fences, bool waitAll = true)
    {
        if (fences.Length == 0)
        {
            return;
        }

        Span<Pointer<SDL_GPUFence>> fencePointers = stackalloc Pointer<SDL_GPUFence>[fences.Length];
        for (int i = 0; i < fences.Length; i++)
        {
            fencePointers[i] = fences[i].Pointer;
        }

        unsafe
        {
            fixed (Pointer<SDL_GPUFence>* fencePointersPtr = fencePointers)
            {
                SDL3.SDL_WaitForGPUFences(SdlGpuDevice, waitAll, (SDL_GPUFence**)fencePointersPtr, (uint)fences.Length);
            }
        }
    }

    public void Dispose()
    {
        // ClearAndCopy atomically copies and clears under lock.
        // Release methods will find their collections already cleared, so the Remove is a no-op.
        foreach (GraphicsPipeline graphicsPipeline in _graphicsPipelines.ClearAndCopy())
        {
            ReleaseGraphicsPipeline(graphicsPipeline);
        }

        foreach (Shader shader in _shaders.ClearAndCopy())
        {
            ReleaseShader(shader);
        }

        foreach (GpuVertexBuffer vertexBuffer in _vertexBuffers.ClearAndCopy())
        {
            ReleaseVertexBuffer(vertexBuffer);
        }

        foreach (GpuStorageBuffer storageBuffer in _storageBuffers.ClearAndCopy())
        {
            ReleaseStorageBuffer(storageBuffer);
        }

        foreach (Texture texture in _textures.ClearAndCopy())
        {
            ReleaseTexture(texture);
        }

        foreach (Sampler sampler in _samplers.ClearAndCopy())
        {
            ReleaseSampler(sampler);
        }
        
        unsafe
        {
            // TODO: crashes silently - needs investigation
            // SDL3.SDL_DestroyGPUDevice(SdlGpuDevice);
            SdlGpuDevice = null;
        }
    }

}
