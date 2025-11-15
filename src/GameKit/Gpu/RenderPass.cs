using System.Diagnostics;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class RenderPass: IRenderPass
{
    internal Pointer<SDL_GPURenderPass> NativePointer { get; private set; }
    private uint? _verticesCount = null;
    

    internal RenderPass(Pointer<SDL_GPURenderPass> nativePointer)
    {
        NativePointer = nativePointer;
    }

    public void BindGraphicsPipeline(GraphicsPipeline graphicsPipeline)
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL3.SDL_BindGPUGraphicsPipeline(NativePointer, graphicsPipeline.Pointer);
        }
    }
    
    private void BindVertexBuffer<TVertexType>(uint slot, GpuVertexBuffer<TVertexType> buffer) where TVertexType : unmanaged, IVertexType
    {
        ThrowIfDisposed();
        unsafe
        {
            SDL_GPUBufferBinding sdlGpuBufferBinding = new SDL_GPUBufferBinding { buffer = buffer.SdlVertexBuffer, offset = 0 };
            SDL3.SDL_BindGPUVertexBuffers(NativePointer, slot, &sdlGpuBufferBinding, 1);
        }
    }

    public void BindVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType
    {
        ThrowIfDisposed();
        
        _verticesCount = (uint)buffer.Size;
        BindVertexBuffer(0, buffer);
    }
    

    public void BindFragmentSamplers(ReadOnlySpan<Texture> textures, Sampler sampler, uint slot = 0)
    {
        ThrowIfDisposed();

        unsafe {
            SDL_GPUTextureSamplerBinding* sdlGpuBufferBindings =
                stackalloc SDL_GPUTextureSamplerBinding[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                sdlGpuBufferBindings[i] = new SDL_GPUTextureSamplerBinding
                    { texture = textures[i].SdlGpuTexture, sampler = sampler.Pointer };
            }

            SDL3.SDL_BindGPUFragmentSamplers(NativePointer, slot, sdlGpuBufferBindings, (uint)textures.Length);
        }
    }

    public void BindFragmentSampler(Texture texture, Sampler sampler)
    {
        ThrowIfDisposed();
        
        ReadOnlySpan<Texture> textures = [texture];
        BindFragmentSamplers(textures, sampler, 0);
    }
    
    public void DrawPrimitive()
    {
        ThrowIfDisposed();
        Debug.Assert(_verticesCount != null, nameof(_verticesCount) + " != null");
        unsafe
        {
            SDL3.SDL_DrawGPUPrimitives(NativePointer, _verticesCount.Value, 1, 0, 0);
        }
    }

    public bool IsDefault()
    {
        return NativePointer.IsNull();
    }
    
    public void Dispose()
    {
        if (!NativePointer.IsNull())
        {
            unsafe
            {
                SDL3.SDL_EndGPURenderPass(NativePointer);
            }
            NativePointer = Pointer<SDL_GPURenderPass>.Null;
        }
    }
    
    private void ThrowIfDisposed()
    {
        if (NativePointer.IsNull())
            throw new ObjectDisposedException(nameof(RenderPass));
    }
}
