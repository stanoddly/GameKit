using System.Runtime.CompilerServices;
using GameKit.Utilities;
using SDL;

namespace GameKit.Gpu;

public interface ICommandBuffer: IDisposable
{
    IRenderPassBuilder RenderPassBuilder { get; }
    void PushFragmentUniformData<TType>(uint slot, TType variable) where TType : unmanaged;
    void PushVertexUniformData<TType>(uint slot, TType variable) where TType : unmanaged;
    void BlitTextures(Texture source, Texture destination);
    ICopyPass CreateCopyPass();
    IRenderPass CreateRenderPass(List<Texture> colorTargets, List<ColorTargetSettings> colorTargetSettings, Texture? depthBuffer, DepthBufferSettings depthBufferSettings);
}

public class CommandBuffer: ICommandBuffer
{
    private readonly GpuDevice _gpuDevice;
    internal Pointer<SDL_GPUCommandBuffer> SdlGpuCommandBuffer { get; private set; }
    public IRenderPassBuilder RenderPassBuilder { get; }

    internal CommandBuffer(GpuDevice gpuDevice, Pointer<SDL_GPUCommandBuffer> sdlCommandBuffer)
    {
        _gpuDevice = gpuDevice;
        SdlGpuCommandBuffer = sdlCommandBuffer;
        RenderPassBuilder = new RenderPassBuilder(this);
    }

    internal void Submit()
    {
        ThrowIfDisposed();
        unsafe
        {
            // TODO: error handling
            SDL3.SDL_SubmitGPUCommandBuffer(SdlGpuCommandBuffer);
            SdlGpuCommandBuffer = Pointer<SDL_GPUCommandBuffer>.Null;
        }
    }
    
    public void PushFragmentUniformData<TType>(uint slot, TType variable) where TType : unmanaged
    {
        ThrowIfDisposed();
        unsafe
        {
            IntPtr data = new IntPtr(Unsafe.AsPointer(ref variable));
            uint size = (uint)Unsafe.SizeOf<TType>();
            SDL3.SDL_PushGPUFragmentUniformData(SdlGpuCommandBuffer, slot, data, size);
        }
    }
    
    public void PushVertexUniformData<TType>(uint slot, TType variable) where TType : unmanaged
    {
        ThrowIfDisposed();
        unsafe
        {
            IntPtr data = new IntPtr(Unsafe.AsPointer(ref variable));
            uint size = (uint)Unsafe.SizeOf<TType>();
            SDL3.SDL_PushGPUVertexUniformData(SdlGpuCommandBuffer, slot, data, size);
        }
    }

    public IRenderPass CreateRenderPass(List<Texture> colorTargets, List<ColorTargetSettings> colorTargetSettings, Texture? depthBuffer, DepthBufferSettings depthBufferSettings)
    {
        ThrowIfDisposed();
        
        Span<SDL_GPUColorTargetInfo> colorTargetInfos = stackalloc SDL_GPUColorTargetInfo[colorTargets.Count];
            
        for (int i = 0; i < colorTargets.Count; i++)
        {
            Texture colorTarget = colorTargets[i];
            ColorTargetSettings colorTargetSetting = colorTargetSettings[i];

            colorTargetInfos[i] = new SDL_GPUColorTargetInfo
            {
                texture = colorTarget.SdlGpuTexture,
                clear_color = colorTargetSetting.ClearColorValue,
                load_op = (SDL_GPULoadOp)colorTargetSetting.LoadOperation,
                store_op = (SDL_GPUStoreOp)colorTargetSetting.StoreOperation
            };
        }
        
        Pointer<SDL_GPUTexture> depthBufferPointer = Pointer<SDL_GPUTexture>.Null;

        if (depthBuffer != null)
        {
            depthBufferPointer = depthBuffer.SdlGpuTexture;
        }
        
        return CreateMultipleRenderTargetsPassInternal(
            colorTargetInfos,
            depthBufferPointer,
            depthBufferSettings);
    }

    private RenderPass CreateMultipleRenderTargetsPassInternal(
        ReadOnlySpan<SDL_GPUColorTargetInfo> colorTargetInfos,
        Pointer<SDL_GPUTexture> depthBufferPointer,
        DepthBufferSettings depthBufferSettings)
    {
        ThrowIfDisposed();
        
        unsafe
        {
            fixed (SDL_GPUColorTargetInfo* colorTargetInfosPtr = colorTargetInfos)
            {
                if (depthBufferPointer.IsNull())
                {
                    SDL_GPURenderPass* gpuRenderPass = SDL3.SDL_BeginGPURenderPass(
                        SdlGpuCommandBuffer,
                        colorTargetInfosPtr,
                        (uint)colorTargetInfos.Length,
                        null);
                    
                    return new RenderPass(gpuRenderPass);
                }
                else
                {
                    SDL_GPUDepthStencilTargetInfo depthStencilTargetInfo = new SDL_GPUDepthStencilTargetInfo
                    {
                        texture = depthBufferPointer,
                        clear_depth = depthBufferSettings.ClearDepthValue,
                        load_op = (SDL_GPULoadOp)depthBufferSettings.DepthBufferLoadOperation,
                        store_op = (SDL_GPUStoreOp)depthBufferSettings.DepthBufferStoreOperation,
                        stencil_load_op = (SDL_GPULoadOp)depthBufferSettings.StencilLoadOperation,
                        stencil_store_op = (SDL_GPUStoreOp)depthBufferSettings.StencilStoreOperation,
                        clear_stencil = depthBufferSettings.ClearStencilValue
                    };
                    
                    SDL_GPURenderPass* gpuRenderPass = SDL3.SDL_BeginGPURenderPass(
                        SdlGpuCommandBuffer,
                        colorTargetInfosPtr,
                        (uint)colorTargetInfos.Length,
                        &depthStencilTargetInfo);
                    
                    return new RenderPass(gpuRenderPass);
                }
            }
        }
    }

    public void BlitTextures(Texture source, Texture destination)
    {
        ThrowIfDisposed();
        
        unsafe
        {
            SDL_GPUBlitRegion sourceRegion = new SDL_GPUBlitRegion
            {
                texture = source.SdlGpuTexture,
                x = 0,
                y = 0,
                w = source.Size.Width,
                h = source.Size.Height
            };

            SDL_GPUBlitRegion destinationRegion = new SDL_GPUBlitRegion
            {
                texture = destination.SdlGpuTexture,
                x = 0,
                y = 0,
                w = destination.Size.Width,
                h = destination.Size.Height
            };

            SDL_GPUBlitInfo blitInfo = new SDL_GPUBlitInfo
            {
                source = sourceRegion,
                destination = destinationRegion,
                load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
                flip_mode = SDL_FlipMode.SDL_FLIP_NONE,
                filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
            };

            SDL3.SDL_BlitGPUTexture(SdlGpuCommandBuffer, &blitInfo);
        }
    }

    public void Dispose()
    {
        if (!SdlGpuCommandBuffer.IsNull())
        {
            Submit();
            SdlGpuCommandBuffer = Pointer<SDL_GPUCommandBuffer>.Null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (SdlGpuCommandBuffer.IsNull())
        {
            throw new ObjectDisposedException(nameof(CommandBuffer));
        }
    }

    public ICopyPass CreateCopyPass()
    {
        unsafe
        {
            SDL_GPUCopyPass* copyPass = SDL3.SDL_BeginGPUCopyPass(SdlGpuCommandBuffer);
            return new CopyPass(_gpuDevice, copyPass);
        }
    }
}
