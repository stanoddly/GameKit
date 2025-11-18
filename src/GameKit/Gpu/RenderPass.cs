using System.Diagnostics;
using GameKit.ShaderCommon;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public struct RenderPassValidator
{
    private uint _verticesCount = 0;
    private VertexTypeId _vertexBufferVertexType = VertexTypeId.Null;
    private GraphicsPipeline? _graphicsPipeline = null;

    private ShaderBindingCounts _fragmentShaderBindingCounts = new();
    private ShaderBindingCounts _vertexShaderBindingCounts = new();

    public RenderPassValidator()
    {
    }
}

public class RenderPass: IRenderPass
{
    private CommandBuffer _commandBuffer;
    internal Pointer<SDL_GPURenderPass> NativePointer { get; private set; }

    private uint _verticesCount = 0;
    private VertexTypeId _vertexBufferVertexType = VertexTypeId.Null;
    private GraphicsPipeline? _graphicsPipeline = null;

    private ShaderBindingCounts _fragmentShaderBindingCounts = new();
    private ShaderBindingCounts _vertexShaderBindingCounts = new();

    internal RenderPass(CommandBuffer commandBuffer, Pointer<SDL_GPURenderPass> nativePointer)
    {
        _commandBuffer = commandBuffer;
        NativePointer = nativePointer;
    }

    public void BindGraphicsPipeline(GraphicsPipeline graphicsPipeline)
    {
        ThrowIfDisposed();

        _graphicsPipeline = graphicsPipeline;
        
        unsafe
        {
            SDL3.SDL_BindGPUGraphicsPipeline(NativePointer, graphicsPipeline.Pointer);
        }
    }
    
    private void BindVertexBuffer<TVertexType>(uint slot, GpuVertexBuffer<TVertexType> buffer) where TVertexType : unmanaged, IVertexType
    {
        ThrowIfDisposed();
        _vertexBufferVertexType = VertexTypeId<TVertexType>.Value;

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

        byte numSamplers = (byte)Math.Max(_fragmentShaderBindingCounts.NumSamplers, slot + textures.Length);

        _fragmentShaderBindingCounts = _fragmentShaderBindingCounts with { NumSamplers = numSamplers };

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

        ValidateDraw();
        
        unsafe
        {
            SDL3.SDL_DrawGPUPrimitives(NativePointer, _verticesCount, 1, 0, 0);
        }
    }

    private void ValidateDraw()
    {
        if (_graphicsPipeline == null)
        {
            throw new InvalidOperationException(
                $"{nameof(GraphicsPipeline)} must be bound.");
        }

        if (_graphicsPipeline.VertexTypeId != _vertexBufferVertexType)
        {
            throw new InvalidOperationException(
                $"TVertexType of both bound {nameof(GraphicsPipeline)} and VertexBuffer must be the same.");
        }

        if (_verticesCount == 0)
        {
            throw new InvalidOperationException("Bound VertexBuffer is empty.");
        }

        ShaderBindingLayoutValidator.ValidateBindingCounts(_graphicsPipeline.FragmentShader.BindingLayout.BindingCounts,
            _fragmentShaderBindingCounts);
        
        ShaderBindingLayoutValidator.ValidateUniformSlotSizes(_graphicsPipeline.FragmentShader.BindingLayout.UniformSlotSizes,
            _commandBuffer.FragmentShaderUniformSlotSizes);
        
        ShaderBindingLayoutValidator.ValidateUniformSlotSizes(_graphicsPipeline.VertexShader.BindingLayout.UniformSlotSizes,
            _commandBuffer.VertexShaderUniformSlotSizes);
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
