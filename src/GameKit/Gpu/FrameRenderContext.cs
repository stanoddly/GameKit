using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public readonly struct ColorTargetInfo
{
    private readonly SDL_GPUColorTargetInfo _sdlGpuColorTargetInfo;

    public ColorTargetInfo(SDL_GPUColorTargetInfo sdlGpuColorTargetInfo)
    {
        _sdlGpuColorTargetInfo = sdlGpuColorTargetInfo;
    }

    public static implicit operator SDL_GPUColorTargetInfo(ColorTargetInfo colorTargetInfo)
    {
        return colorTargetInfo._sdlGpuColorTargetInfo;
    }
    
    public static implicit operator ColorTargetInfo(SDL_GPUColorTargetInfo gpuColorTargetInfo)
    {
        return new ColorTargetInfo(gpuColorTargetInfo);
    }
}

public class Texture
{
    public Pointer<SDL_GPUTexture> Pointer { get; }
    public int Width { get; }
    public int Height { get; }

    public Texture(Pointer<SDL_GPUTexture> pointer, int width, int height)
    {
        Pointer = pointer;
        Width = width;
        Height = height;
    }
}

public struct FrameRenderPass: IDisposable
{
    public Pointer<SDL_GPURenderPass> SdlGpuRenderPass { get; private set; }
    public Pointer<SDL_GPUGraphicsPipeline> SdlGpuGraphicsPipeline { get; private set; }
    private int? _boundVertexBufferSize;
    
    public FrameRenderPass(Pointer<SDL_GPURenderPass> sdlGpuRenderPass, Pointer<SDL_GPUGraphicsPipeline> sdlGpuGraphicsPipeline)
    {
        SdlGpuRenderPass = sdlGpuRenderPass;
        SdlGpuGraphicsPipeline = sdlGpuGraphicsPipeline;
    }

    public void BindVertexBuffer<TVertexType>(VertexBuffer<TVertexType> buffer) where TVertexType : unmanaged, IVertexType
    {
        _boundVertexBufferSize = buffer.Size;
        unsafe
        {
            SDL_GPUBufferBinding sdlGpuBufferBinding = new SDL_GPUBufferBinding { buffer = buffer.SdlVertexBuffer, offset = 0 };
            SDL3.SDL_BindGPUVertexBuffers(SdlGpuRenderPass, 0, &sdlGpuBufferBinding, 1);
        }
    }
    
    public void BindFragmentSampler(Texture texture, Sampler sampler)
    {
        unsafe
        {
            SDL_GPUTextureSamplerBinding sdlGpuBufferBinding = new SDL_GPUTextureSamplerBinding { texture = texture.Pointer, sampler = sampler.Pointer };
            // TODO: first slot 0!!
            SDL3.SDL_BindGPUFragmentSamplers(SdlGpuRenderPass, 0, &sdlGpuBufferBinding, 1);
        }
    }

    public void DrawPrimitive()
    {
        if (_boundVertexBufferSize == null)
        {
            throw new InvalidOperationException("No vertex bound, did you forget to call BindVertexBuffer? You may use SDL_DrawGPUPrimitives if you know what you are doing.");
        }
        unsafe
        {
            SDL3.SDL_DrawGPUPrimitives(SdlGpuRenderPass, (uint)_boundVertexBufferSize.Value, 1, 0, 0);
        }
    }
    
    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_EndGPURenderPass(SdlGpuRenderPass);
        }
    }
}

// TODO: this is going to be a ref structure with IDisposable, that's possible in .NET 9
public readonly struct FrameRenderContext: IDisposable
{
    public required GpuCommandBuffer GpuCommandBuffer { get; init; }
    public required Texture SwapchainTexture { get; init; }
    public required ColorTargetInfo ColorTargetInfo { get; init; }
    public required (uint, uint) Size { get; init; }

    public FrameRenderPass CreateRenderPass(GraphicsPipeline graphicsPipeline)
    {
        unsafe
        {
            SDL_GPUColorTargetInfo sdlGpuColorTargetInfo = ColorTargetInfo;
            SDL_GPURenderPass* gpuRenderPass = SDL3.SDL_BeginGPURenderPass(GpuCommandBuffer.Pointer, &sdlGpuColorTargetInfo, 1, null);
            SDL3.SDL_BindGPUGraphicsPipeline(gpuRenderPass, graphicsPipeline.Pointer);
            return new FrameRenderPass(gpuRenderPass, graphicsPipeline.Pointer);
        }
    }
    
    public void Dispose()
    {
        unsafe
        {
            SDL3.SDL_SubmitGPUCommandBuffer(GpuCommandBuffer.Pointer);
        }
    }
}