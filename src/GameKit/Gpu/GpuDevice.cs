using System.Runtime.CompilerServices;
using GameKit.Common;
using GameKit.Shaders;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

internal class GpuDevice : IGpuDevice
{
    private readonly List<Texture> _textures = new();
    private readonly List<GpuVertexBuffer> _vertexBuffers = new();
    private readonly List<Sampler> _samplers = new();
    private readonly List<GraphicsPipeline> _graphicsPipelines = new();
    private readonly List<Shader> _shaders = new();
    private SDL_PropertiesID _dummyProps;

    internal Pointer<SDL_GPUDevice> SdlGpuDevice { get; private set; }

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
                sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
                // TODO: this is actually SDL bug
                props = _dummyProps
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
                sample_count = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
                // TODO: this is actually SDL bug
                props = _dummyProps
            };
            
            Pointer<SDL_GPUTexture> rawTexture = SDL3.SDL_CreateGPUTexture(SdlGpuDevice, &info);
            SdlError.ThrowOnNull(rawTexture);
            
            Texture texture = new UserTexture(this, rawTexture, size, format);
            _textures.Add(texture);

            return texture;
        }
    }

    public void RegisterTexture(Texture texture)
    {
        _textures.Add(texture);
    }

    public void RegisterVertexBuffer(GpuVertexBuffer vertexBuffer)
    {
        _vertexBuffers.Add(vertexBuffer);
    }
    
    public void RegisterGraphicsPipeline(GraphicsPipeline graphicsPipeline)
    {
        _graphicsPipelines.Add(graphicsPipeline);
    }

    public void RegisterShader(Shader shader)
    {
        _shaders.Add(shader);
    }

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

    public void Dispose()
    {
        // The copy and clear each time is to be able to iterate over each element safely while not having to delete
        // individual item on each Release (because it's empty).
        Span<GraphicsPipeline> graphicsPipelineCopy = _graphicsPipelines.ToArray();
        _graphicsPipelines.Clear();

        foreach (GraphicsPipeline graphicsPipeline in graphicsPipelineCopy)
        {
            ReleaseGraphicsPipeline(graphicsPipeline);
        }
        
        Span<Shader> shadersCopy = _shaders.ToArray();
        _shaders.Clear();

        foreach (Shader shader in shadersCopy)
        {
            ReleaseShader(shader);
        }

        Span<GpuVertexBuffer> vertexBuffersCopy = _vertexBuffers.ToArray();
        _vertexBuffers.Clear();

        foreach (GpuVertexBuffer vertexBuffer in vertexBuffersCopy)
        {
            ReleaseVertexBuffer(vertexBuffer);
        }
        
        Span<Texture> texturesCopy = _textures.ToArray();
        _textures.Clear();

        foreach (Texture texture in texturesCopy)
        {
            ReleaseTexture(texture);
        }

        Span<Sampler> samplersCopy = _samplers.ToArray();
        _samplers.Clear();
        foreach (Sampler sampler in samplersCopy)
        {
            ReleaseSampler(sampler);
        }
        
        unsafe
        {
            // TODO: this is silently crashing since probably not all resources are cleaned appropriately (?)
            //SDL3.SDL_DestroyGPUDevice(SdlGpuDevice);
            SdlGpuDevice = null;
        }
    }

    public void Initialize()
    {
        // TODO: this is actually SDL bug
        // https://github.com/libsdl-org/SDL/issues/12295
        _dummyProps = SDL3.SDL_CreateProperties();
    }
}
