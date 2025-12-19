using System.Diagnostics;
using GameKit.ShaderCommon;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public class RenderPass<TValidator> : IRenderPass
    where TValidator : IRenderPassValidator<TValidator>
{
    private Pointer<SDL_GPURenderPass> _nativePointer;
    private uint _verticesCount = 0;
    private TValidator _validator;
    
    private ShaderBindingCounts _fragmentShaderBindingCounts;
    private ShaderBindingCounts _vertexShaderBindingCounts;
    
    public ShaderBindingCounts FragmentShaderBindingCounts => _fragmentShaderBindingCounts;
    public ShaderBindingCounts VertexShaderBindingCounts => _vertexShaderBindingCounts;

    internal RenderPass(CommandBuffer commandBuffer, Pointer<SDL_GPURenderPass> nativePointer)
    {
        _nativePointer = nativePointer;
        _validator = TValidator.Create(commandBuffer);
    }

    public void BindGraphicsPipeline(GraphicsPipeline graphicsPipeline)
    {
        ThrowIfDisposed();

        _validator.OnBindGraphicsPipeline(this, graphicsPipeline);

        unsafe
        {
            SDL3.SDL_BindGPUGraphicsPipeline(_nativePointer, graphicsPipeline.Pointer);
        }
    }
    
    private void BindVertexBuffer<TVertexType>(uint slot, GpuVertexBuffer<TVertexType> buffer) where TVertexType : unmanaged, IVertexType
    {
        ThrowIfDisposed();

        _verticesCount = (uint)buffer.BufferSize;

        unsafe
        {
            SDL_GPUBufferBinding sdlGpuBufferBinding = new SDL_GPUBufferBinding { buffer = buffer.SdlVertexBuffer, offset = 0 };
            SDL3.SDL_BindGPUVertexBuffers(_nativePointer, slot, &sdlGpuBufferBinding, 1);
        }
    }

    public void BindVertexBuffer<TVertexType>(GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType
    {
        ThrowIfDisposed();

        _validator.OnBindVertexBuffer(this, buffer);

        BindVertexBuffer(0, buffer);
    }
    

    public void BindVertexSamplers(ReadOnlySpan<Texture> textures, Sampler sampler, uint slot = 0)
    {
        ThrowIfDisposed();
        
        byte numSamplers = (byte)Math.Max(_vertexShaderBindingCounts.NumSamplers, slot + textures.Length);
        _vertexShaderBindingCounts = _vertexShaderBindingCounts with { NumSamplers = numSamplers };

        _validator.OnBindVertexSamplers(this, slot, textures.Length);

        unsafe {
            SDL_GPUTextureSamplerBinding* sdlGpuBufferBindings =
                stackalloc SDL_GPUTextureSamplerBinding[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                sdlGpuBufferBindings[i] = new SDL_GPUTextureSamplerBinding
                    { texture = textures[i].SdlGpuTexture, sampler = sampler.Pointer };
            }

            SDL3.SDL_BindGPUVertexSamplers(_nativePointer, slot, sdlGpuBufferBindings, (uint)textures.Length);
        }
    }

    public void BindFragmentSamplers(ReadOnlySpan<Texture> textures, Sampler sampler, uint slot = 0)
    {
        ThrowIfDisposed();
        
        byte numSamplers = (byte)Math.Max(_fragmentShaderBindingCounts.NumSamplers, slot + textures.Length);
        _fragmentShaderBindingCounts = _fragmentShaderBindingCounts with { NumSamplers = numSamplers };

        _validator.OnBindFragmentSamplers(this, slot, textures.Length);

        unsafe {
            SDL_GPUTextureSamplerBinding* sdlGpuBufferBindings =
                stackalloc SDL_GPUTextureSamplerBinding[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                sdlGpuBufferBindings[i] = new SDL_GPUTextureSamplerBinding
                    { texture = textures[i].SdlGpuTexture, sampler = sampler.Pointer };
            }

            SDL3.SDL_BindGPUFragmentSamplers(_nativePointer, slot, sdlGpuBufferBindings, (uint)textures.Length);
        }
    }

    public void BindFragmentSampler(Texture texture, Sampler sampler)
    {
        ThrowIfDisposed();

        ReadOnlySpan<Texture> textures = [texture];
        BindFragmentSamplers(textures, sampler, 0);
    }

    public void BindFragmentSamplerArray(TextureArray textureArray, Sampler sampler, uint slot = 0)
    {
        ThrowIfDisposed();

        byte numSamplers = (byte)Math.Max(_fragmentShaderBindingCounts.NumSamplers, slot + 1);
        _fragmentShaderBindingCounts = _fragmentShaderBindingCounts with { NumSamplers = numSamplers };

        _validator.OnBindFragmentSamplers(this, slot, 1);

        unsafe
        {
            SDL_GPUTextureSamplerBinding sdlGpuBufferBinding = new SDL_GPUTextureSamplerBinding
            {
                texture = textureArray.SdlGpuTexture,
                sampler = sampler.Pointer
            };

            SDL3.SDL_BindGPUFragmentSamplers(_nativePointer, slot, &sdlGpuBufferBinding, 1);
        }
    }
    
    public void DrawPrimitive()
    {
        ThrowIfDisposed();

        _validator.OnDrawPrimitive(this);

        unsafe
        {
            SDL3.SDL_DrawGPUPrimitives(_nativePointer, _verticesCount, 1, 0, 0);
        }
    }

    public bool IsDefault()
    {
        return _nativePointer.IsNull();
    }
    
    public void Dispose()
    {
        if (!_nativePointer.IsNull())
        {
            unsafe
            {
                SDL3.SDL_EndGPURenderPass(_nativePointer);
            }
            _nativePointer = Pointer<SDL_GPURenderPass>.Null;
        }
    }
    
    private void ThrowIfDisposed()
    {
        if (_nativePointer.IsNull())
            throw new ObjectDisposedException(nameof(RenderPass));
    }
}

/// <summary>
/// Non-generic render pass using the default RenderPassValidator with full validation checks.
/// </summary>
public class RenderPass : RenderPass<RenderPassValidator>
{
    internal RenderPass(CommandBuffer commandBuffer, Pointer<SDL_GPURenderPass> nativePointer)
        : base(commandBuffer, nativePointer)
    {
    }
}
