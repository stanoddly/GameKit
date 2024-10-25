using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL;

namespace GameKit;

public enum PrimitiveType
{
    TriangleList = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,
    TriangleStrip = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLESTRIP,
    LineList = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_LINELIST,
    LineStrip = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_LINESTRIP,
    PointList = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_POINTLIST
}

public enum SampleCount
{
    Count1 = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_1,
    Count2 = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_2,
    Count4 = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_4,
    Count8 = SDL_GPUSampleCount.SDL_GPU_SAMPLECOUNT_8
}

public enum CompareOperation
{
    Invalid = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_INVALID,
    Never = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_NEVER,
    Less = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_LESS,
    Equal = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_EQUAL,
    LessOrEqual = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_LESS_OR_EQUAL,
    Greater = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_GREATER,
    NotEqual = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_NOT_EQUAL,
    GreaterOrEqual = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_GREATER_OR_EQUAL,
    Always = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_ALWAYS
}

public enum StencilOperation
{
    Invalid = SDL_GPUStencilOp.SDL_GPU_STENCILOP_INVALID,
    Keep = SDL_GPUStencilOp.SDL_GPU_STENCILOP_KEEP,
    Zero = SDL_GPUStencilOp.SDL_GPU_STENCILOP_ZERO,
    Replace = SDL_GPUStencilOp.SDL_GPU_STENCILOP_REPLACE,
    IncrementAndClamp = SDL_GPUStencilOp.SDL_GPU_STENCILOP_INCREMENT_AND_CLAMP,
    DecrementAndClamp = SDL_GPUStencilOp.SDL_GPU_STENCILOP_DECREMENT_AND_CLAMP,
    Invert = SDL_GPUStencilOp.SDL_GPU_STENCILOP_INVERT,
    IncrementAndWrap = SDL_GPUStencilOp.SDL_GPU_STENCILOP_INCREMENT_AND_WRAP,
    DecrementAndWrap = SDL_GPUStencilOp.SDL_GPU_STENCILOP_DECREMENT_AND_WRAP,
}

public readonly record struct StencilOperationState(
    StencilOperation Fail,
    StencilOperation Pass,
    StencilOperation DepthFail,
    CompareOperation Compare)
{
    public static implicit operator SDL_GPUStencilOpState(in StencilOperationState stencilOperationState)
    {
        return new SDL_GPUStencilOpState
        {
            compare_op = (SDL_GPUCompareOp)stencilOperationState.Compare,
            depth_fail_op = (SDL_GPUStencilOp)stencilOperationState.DepthFail,
            fail_op = (SDL_GPUStencilOp)stencilOperationState.Fail,
            pass_op = (SDL_GPUStencilOp)stencilOperationState.Pass
        };
    }
}

internal struct PipelineBuilderInfo
{
    public PipelineBuilderInfo()
    {
    }

    public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.TriangleList;
    public List<SDL_GPUColorTargetDescription> SdlGpuColorTargetDescriptions { get; } = new();
    public List<SDL_GPUVertexAttribute> SdlGpuVertexAttributes { get; } = new();
    public List<SDL_GPUVertexBufferDescription> SdlGpuVertexBufferDescriptions { get; } = new();
    public SDL_GPUMultisampleState SdlGpuMultisampleState { get; set; }
    public SDL_GPUDepthStencilState SdlGpuDepthStencilState = new();
    
    public Shader? VertexShader { get; set; } = null;
    public Shader? FragmentShader { get; set; } = null;

    public void Reset()
    {
        SdlGpuColorTargetDescriptions.Clear();
        SdlGpuVertexAttributes.Clear();
        SdlGpuVertexBufferDescriptions.Clear();
        VertexShader = null;
        FragmentShader = null;
        PrimitiveType = PrimitiveType.TriangleList;
        SdlGpuMultisampleState = new();
        SdlGpuDepthStencilState = new();
    }
}

public class GraphicsPipelineBuilder
{
    private GpuDevice _gpuDevice;
    private Window _window;
    private PipelineBuilderInfo _info = new();

    public GraphicsPipelineBuilder(GpuDevice gpuDevice, Window window)
    {
        _gpuDevice = gpuDevice;
        _window = window;
    }

    public GraphicsPipelineBuilder AddColorFormatFromDisplay()
    {
        unsafe
        {
            SDL_GPUTextureFormat swapchainFormat =
                SDL3.SDL_GetGPUSwapchainTextureFormat(_gpuDevice.SdlGpuDevice, _window.Pointer);
            
            _info.SdlGpuColorTargetDescriptions.Add(new SDL_GPUColorTargetDescription
            {
                format = swapchainFormat
            });
        }

        return this;
    }

    public GraphicsPipelineBuilder AddVertexBufferConfig<TVertexType>(int? instanceStepRate = default) where TVertexType : unmanaged, IVertexType
    {
        uint vertexTypeSizeBytes = (uint)Unsafe.SizeOf<TVertexType>();

        SDL_GPUVertexInputRate inputRate = SDL_GPUVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX;
        uint finalInstanceStepRate = 0;
        if (instanceStepRate.HasValue)
        {
            if (instanceStepRate.Value < 1)
            {
                throw new ArgumentException("instanceStepRate must be greater than zero!");
            }

            finalInstanceStepRate = (uint)instanceStepRate.Value;
            inputRate = SDL_GPUVertexInputRate.SDL_GPU_VERTEXINPUTRATE_INSTANCE;
        }

        uint bufferSlot = (uint)_info.SdlGpuVertexBufferDescriptions.Count;
        SDL_GPUVertexBufferDescription sdlGpuVertexBufferDescription = new()
        {
            slot = bufferSlot,
            input_rate = inputRate,
            instance_step_rate = finalInstanceStepRate,
            pitch = vertexTypeSizeBytes
        };
        _info.SdlGpuVertexBufferDescriptions.Add(sdlGpuVertexBufferDescription);

        uint location = 0;
        uint offset = 0;
        
        foreach (VertexElementFormat vertexElementFormat in TVertexType.VertexElements)
        {
            _info.SdlGpuVertexAttributes.Add(new SDL_GPUVertexAttribute
            {
                buffer_slot = bufferSlot,
                format = (SDL_GPUVertexElementFormat)vertexElementFormat,
                location = location,
                offset = offset
            });

            // TODO: we may assert that the number of bytes is not higher than Unsafe.Size<TVertexType>()
            offset += (uint)vertexElementFormat.GetNumberOfBytes();
            location++;
        }

        return this;
    }
    
    public GraphicsPipelineBuilder SetShaders(Shader vertexShader, Shader fragmentShader)
    {
        if (vertexShader.Stage != ShaderStage.Vertex)
        {
            throw new ArgumentException("vertexShader.Stage != ShaderStage.Vertex");
        }
        
        if (fragmentShader.Stage != ShaderStage.Fragment)
        {
            throw new ArgumentException("fragmentShader.Stage != ShaderStage.Fragment");
        }

        _info.VertexShader = vertexShader;
        _info.FragmentShader = fragmentShader;

        return this;
    }

    public GraphicsPipelineBuilder SetPrimitiveType(PrimitiveType primitiveType)
    {
        _info.PrimitiveType = primitiveType;
        return this;
    }

    public GraphicsPipelineBuilder EnableMultiSampling(SampleCount sampleCount, UInt32? mask = null)
    {
        // TODO: check the value with SDL_GPUTextureSupportsSampleCount
        _info.SdlGpuMultisampleState = _info.SdlGpuMultisampleState with
        {
            sample_count = (SDL_GPUSampleCount)sampleCount,
            enable_mask = mask.HasValue ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE,
            sample_mask = mask ?? 0
        };

        return this;
    }
    
    // public GraphicsPipelineBuilder EnableBestSupportedMultiSampling()
    // {
    //     throw new NotImplementedException();
    // }
    
    public GraphicsPipelineBuilder EnableDepthTesting(CompareOperation compareOperation = CompareOperation.Less)
    {
        _info.SdlGpuDepthStencilState = _info.SdlGpuDepthStencilState with
        {
            enable_depth_test = SDL_bool.SDL_TRUE,
            compare_op = (SDL_GPUCompareOp)compareOperation
        };
        
        return this;
    }
    
    public GraphicsPipelineBuilder EnableDepthWriting()
    {
        _info.SdlGpuDepthStencilState = _info.SdlGpuDepthStencilState with
        {
            enable_depth_test = SDL_bool.SDL_TRUE,
            enable_depth_write = SDL_bool.SDL_TRUE
        };
        return this;
    }
    
    public GraphicsPipelineBuilder EnableStencilTesting(in StencilOperationState frontFacing, CompareOperation compareOperation, byte compareMask=0xFF, byte writeMask=0xFF)
    {
        _info.SdlGpuDepthStencilState = _info.SdlGpuDepthStencilState with
        {
            enable_stencil_test = SDL_bool.SDL_TRUE,
            compare_op = (SDL_GPUCompareOp)compareOperation,
            front_stencil_state = frontFacing,
            compare_mask = compareMask,
            write_mask = writeMask
        };
        return this;
    }
    
    public GraphicsPipelineBuilder EnableStencilTesting(in StencilOperationState frontFacing, in StencilOperationState backFacing, CompareOperation compareOperation, byte compareMask=0xFF, byte writeMask=0xFF)
    {
        _info.SdlGpuDepthStencilState = _info.SdlGpuDepthStencilState with
        {
            enable_stencil_test = SDL_bool.SDL_TRUE,
            compare_op = (SDL_GPUCompareOp)compareOperation,
            front_stencil_state = frontFacing,
            back_stencil_state = backFacing,
            compare_mask = compareMask,
            write_mask = writeMask
        };
        return this;
    }
    
    public GraphicsPipeline Build()
    {
        Span<SDL_GPUColorTargetDescription> sdlGpuColorTargetDescriptions =
            CollectionsMarshal.AsSpan(_info.SdlGpuColorTargetDescriptions);
        Span<SDL_GPUVertexBufferDescription> sdlGpuVertexBufferDescription =
            CollectionsMarshal.AsSpan(_info.SdlGpuVertexBufferDescriptions);
        Span<SDL_GPUVertexAttribute> sdlGpuVertexAttributes = CollectionsMarshal.AsSpan(_info.SdlGpuVertexAttributes);

        if (sdlGpuColorTargetDescriptions.Length == 0)
        {
            // TODO: change
            throw new NotImplementedException();
        }
        
        if (sdlGpuVertexBufferDescription.Length == 0)
        {
            // TODO: change
            throw new NotImplementedException();
        }
        
        if (sdlGpuVertexAttributes.Length == 0)
        {
            // TODO: change
            throw new NotImplementedException();
        }

        if (_info.VertexShader?.Pointer.IsNull() ?? false)
        {
            // TODO: change
            throw new NotImplementedException();
        }

        if (_info.FragmentShader?.Pointer.IsNull() ?? false)
        {
            // TODO: change
            throw new NotImplementedException();
        }
        
        unsafe
        {
            fixed (SDL_GPUColorTargetDescription* sdlGpuColorTargetDescriptionsPointer = sdlGpuColorTargetDescriptions)
            fixed (SDL_GPUVertexBufferDescription* sdlGpuVertexBufferDescriptionPointer = sdlGpuVertexBufferDescription)
            fixed (SDL_GPUVertexAttribute* sdlGpuVertexAttributePointer = sdlGpuVertexAttributes)
            {
                SDL_GPUGraphicsPipelineCreateInfo sdlGpuGraphicsPipelineCreateInfo = new()
                {
                    target_info = new()
                    {
                        num_color_targets = (uint)sdlGpuColorTargetDescriptions.Length,
                        color_target_descriptions = sdlGpuColorTargetDescriptionsPointer,
                    },
                    vertex_input_state = new SDL_GPUVertexInputState
                    {
                        num_vertex_buffers = 1,
                        vertex_buffer_descriptions = sdlGpuVertexBufferDescriptionPointer,
                        num_vertex_attributes = (uint)sdlGpuVertexAttributes.Length,
                        vertex_attributes = sdlGpuVertexAttributePointer
                    },
                    primitive_type = (SDL_GPUPrimitiveType)_info.PrimitiveType,
                    vertex_shader = _info.VertexShader!.Pointer,
                    fragment_shader = _info.FragmentShader!.Pointer,
                    multisample_state = _info.SdlGpuMultisampleState,
                    depth_stencil_state = _info.SdlGpuDepthStencilState
                };

                var pipeline = SDL3.SDL_CreateGPUGraphicsPipeline(_gpuDevice.SdlGpuDevice, &sdlGpuGraphicsPipelineCreateInfo);
                if (pipeline == null)
                {
                    throw new GameKitInitializationException(
                        $"SDL_CreateGPUGraphicsPipeline failed: {SDL3.SDL_GetError()}");
                }

                GraphicsPipeline graphicsPipeline = new GraphicsPipeline(pipeline);
                _info.Reset();
                return graphicsPipeline;
            }
        }
    }
}