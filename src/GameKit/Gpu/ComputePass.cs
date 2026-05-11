using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class ComputePass : IComputePass
{
    private Pointer<SDL_GPUComputePass> _nativePointer;

    internal ComputePass(Pointer<SDL_GPUComputePass> nativePointer)
    {
        _nativePointer = nativePointer;
    }

    public void BindComputePipeline(ComputePipeline pipeline)
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL3.SDL_BindGPUComputePipeline(_nativePointer, pipeline.Pointer);
        }
    }

    public void BindSamplers(ReadOnlySpan<Texture> textures, Sampler sampler, uint slot = 0)
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL_GPUTextureSamplerBinding* bindings = stackalloc SDL_GPUTextureSamplerBinding[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                bindings[i] = new SDL_GPUTextureSamplerBinding
                {
                    texture = textures[i].SdlGpuTexture,
                    sampler = sampler.Pointer
                };
            }
            SDL3.SDL_BindGPUComputeSamplers(_nativePointer, slot, bindings, (uint)textures.Length);
        }
    }

    public void BindStorageTextures(ReadOnlySpan<Texture> textures, uint slot = 0)
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL_GPUTexture** sdlTextures = stackalloc SDL_GPUTexture*[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                sdlTextures[i] = textures[i].SdlGpuTexture;
            }
            SDL3.SDL_BindGPUComputeStorageTextures(_nativePointer, slot, sdlTextures, (uint)textures.Length);
        }
    }

    public void BindStorageTexture(Texture texture, uint slot = 0)
    {
        ReadOnlySpan<Texture> textures = [texture];
        BindStorageTextures(textures, slot);
    }

    public void BindStorageBuffers(ReadOnlySpan<GpuStorageBuffer> buffers, uint slot = 0)
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL_GPUBuffer** sdlBuffers = stackalloc SDL_GPUBuffer*[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
            {
                sdlBuffers[i] = buffers[i].SdlBuffer;
            }
            SDL3.SDL_BindGPUComputeStorageBuffers(_nativePointer, slot, sdlBuffers, (uint)buffers.Length);
        }
    }

    public void BindStorageBuffer(GpuStorageBuffer buffer, uint slot = 0)
    {
        ReadOnlySpan<GpuStorageBuffer> buffers = [buffer];
        BindStorageBuffers(buffers, slot);
    }

    public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL3.SDL_DispatchGPUCompute(_nativePointer, groupCountX, groupCountY, groupCountZ);
        }
    }

    public void DispatchIndirect(GpuStorageBuffer buffer, uint offset = 0)
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL3.SDL_DispatchGPUComputeIndirect(_nativePointer, buffer.SdlBuffer, offset);
        }
    }

    public void Dispose()
    {
        if (!_nativePointer.IsNull)
        {
            unsafe
            {
                SDL3.SDL_EndGPUComputePass(_nativePointer);
            }
            _nativePointer = Pointer<SDL_GPUComputePass>.Null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_nativePointer.IsNull)
        {
            throw new ObjectDisposedException(nameof(ComputePass));
        }
    }
}
