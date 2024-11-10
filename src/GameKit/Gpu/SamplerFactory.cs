using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public struct Sampler
{
    public Sampler(Pointer<SDL_GPUSampler> pointer)
    {
        Pointer = pointer;
    }

    public Pointer<SDL_GPUSampler> Pointer { get; }
}

public enum SamplerAddressMode: byte
{
    Repeat = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
    MirroredRepeat = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_MIRRORED_REPEAT,
    ClampToEdge=SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE
}

public readonly struct SamplerAddressModes
{
    public SamplerAddressMode U { get; }
    public SamplerAddressMode V { get; }
    public SamplerAddressMode W { get; }

    public SamplerAddressModes(SamplerAddressMode all)
    {
        U = all;
        V = all;
        W = all;
    }
    
    public SamplerAddressModes(SamplerAddressMode u, SamplerAddressMode v, SamplerAddressMode w)
    {
        U = u;
        V = v;
        W = w;
    }
    
    public static readonly SamplerAddressModes Repeat = new SamplerAddressModes(SamplerAddressMode.Repeat);
    public static readonly SamplerAddressModes MirroredRepeat = new SamplerAddressModes(SamplerAddressMode.MirroredRepeat);
    public static readonly SamplerAddressModes ClampToEdge = new SamplerAddressModes(SamplerAddressMode.ClampToEdge);
}

public enum TextureFilter
{
    Nearest=SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
    Linear=SDL_GPUFilter.SDL_GPU_FILTER_LINEAR,
}

public enum SamplerMipmapMode : byte
{
    Nearest=SDL_GPUSamplerMipmapMode.SDL_GPU_SAMPLERMIPMAPMODE_NEAREST,
    Linear=SDL_GPUSamplerMipmapMode.SDL_GPU_SAMPLERMIPMAPMODE_LINEAR
}

/*public class SamplerFactory
{
    private GpuDevice _gpuDevice;

    public SamplerFactory(GpuDevice gpuDevice)
    {
        _gpuDevice = gpuDevice;
    }

    public Sampler CreateSampler(SamplerAddressModes addressModes)
    {
        SDL_GPUSamplerCreateInfo sdlGpuSamplerCreateInfo = new SDL_GPUSamplerCreateInfo()
        {
            min_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
            mag_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
            mipmap_mode = SDL_GPUSamplerMipmapMode.SDL_GPU_SAMPLERMIPMAPMODE_NEAREST,
            address_mode_u = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
            address_mode_v = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
            address_mode_w = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
        };
    }
    
        public Sampler CreatePixelArtSampler()
   {
       SDL_GPUSamplerCreateInfo sdlGpuSamplerCreateInfo = new SDL_GPUSamplerCreateInfo()
       {
           min_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
           mag_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
           mipmap_mode = SDL_GPUSamplerMipmapMode.SDL_GPU_SAMPLERMIPMAPMODE_NEAREST,
           address_mode_u = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
           address_mode_v = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
           address_mode_w = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_REPEAT,
       };
       unsafe
       {
           Pointer<SDL_GPUSampler> samplerPointer = SDL3.SDL_CreateGPUSampler(_gpuDevice.SdlGpuDevice, &sdlGpuSamplerCreateInfo);

           return new Sampler(samplerPointer);
       }
   }
}*/