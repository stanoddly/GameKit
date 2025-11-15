using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GameKit.Content;
using GameKit.Shaders;
using SDL;

namespace GameKit.Gpu;

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
    Always = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_ALWAYS,

    // for reversed depth buffer
    ReversedLess = Greater,
    ReversedLessOrEqual = GreaterOrEqual,
    ReversedGreater = Less,
    ReversedGreaterOrEqual = LessOrEqual
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
    public SDL_GPUColorTargetBlendState SdlGpuColorTargetBlendState { get; set; }
    public RasterizerState RasterizerState { get; set; } = new() { CullMode = CullMode.Back, FrontFace = FrontFace.Clockwise };
    
    public DepthBufferFormat? DepthBufferFormat { get; set; }
    
    public Shader? VertexShader { get; set; } = null;
    public Shader? FragmentShader { get; set; } = null;
    public Type? VertexBufferType { get; set; } = null;

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
        DepthBufferFormat = null;
        SdlGpuColorTargetBlendState = default;
        // We use left hand coordinates, that's why CLOCKWISE winding order
        RasterizerState = new() { CullMode = CullMode.Back, FrontFace = FrontFace.Clockwise };
        VertexBufferType = null;
    }
}

public class GraphicsPipelineBuilder
{
    private readonly GpuDevice _gpuDevice;
    private readonly IWindow _window;
    private readonly IContentLoader<Shader> _shaderLoader;
    private PipelineBuilderInfo _info = new();

    internal GraphicsPipelineBuilder(GpuDevice gpuDevice, IWindow window, IContentLoader<Shader> shaderLoader)
    {
        _gpuDevice = gpuDevice;
        _window = window;
        _shaderLoader = shaderLoader;
    }

    public GraphicsPipelineBuilder AddColorFormatFromDisplay(in BlendingState? blendingState = null, ColorComponentFlags? colorWriteMask = null)
    {
        AddColorTarget(_window.ColorTargetFormat, blendingState, colorWriteMask);

        return this;
    }
    
    public GraphicsPipelineBuilder AddColorTarget(TextureFormat textureFormat, in BlendingState? blendingState = null, ColorComponentFlags? colorWriteMask = null)
    {
        SDL_GPUColorTargetDescription description = new SDL_GPUColorTargetDescription
        {
            format = (SDL_GPUTextureFormat)textureFormat,
        };

        if (blendingState != null)
        {
            description.blend_state.enable_blend = true;
            description.blend_state.src_color_blendfactor = (SDL_GPUBlendFactor)blendingState.Value.SourceColorBlendFactor;
            description.blend_state.dst_color_blendfactor = (SDL_GPUBlendFactor)blendingState.Value.DestinationColorBlendFactor;
            description.blend_state.color_blend_op = (SDL_GPUBlendOp)blendingState.Value.ColorBlendOp;
            description.blend_state.src_alpha_blendfactor = (SDL_GPUBlendFactor)blendingState.Value.SourceAlphaBlendFactor;
            description.blend_state.dst_alpha_blendfactor = (SDL_GPUBlendFactor)blendingState.Value.DestinationAlphaBlendFactor;
            description.blend_state.alpha_blend_op = (SDL_GPUBlendOp)blendingState.Value.AlphaBlendOp;
        }

        if (colorWriteMask != null)
        {
            description.blend_state.enable_color_write_mask = true;
            description.blend_state.color_write_mask = (SDL_GPUColorComponentFlags)colorWriteMask.Value;
        }
        
        _info.SdlGpuColorTargetDescriptions.Add(description);

        return this;
    }

    public GraphicsPipelineBuilder AddVertexBufferConfigBasedOnBuffer<TVertexType>(GpuVertexBuffer<TVertexType> buffer,
        int? instanceStepRate = default) where TVertexType : unmanaged, IVertexType
    {
        return AddVertexBufferConfig<TVertexType>(instanceStepRate);
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

    public GraphicsPipelineBuilder SetShaders(string vertexShaderPath, string fragmentShaderPath)
    {
        Shader vertexShader = _shaderLoader.Load(vertexShaderPath);
        Shader fragmentShader = _shaderLoader.Load(fragmentShaderPath);
        
        return SetShaders(vertexShader, fragmentShader);
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
            enable_mask = mask.HasValue,
            sample_mask = mask ?? 0
        };

        return this;
    }

    public GraphicsPipelineBuilder EnableDepthTesting(DepthBufferFormat depthBufferFormat, bool write = true, CompareOperation compareOp = CompareOperation.Less)
    {
        return EnableDepthTesting((TextureFormat)depthBufferFormat, write, compareOp);
    }
    
    public GraphicsPipelineBuilder EnableReversedDepthTesting(DepthBufferFormat depthBufferFormat, bool write = true, CompareOperation compareOp = CompareOperation.ReversedLess)
    {
        return EnableDepthTesting((TextureFormat)depthBufferFormat, write, compareOp);
    }
    
    public GraphicsPipelineBuilder EnableReversedDepthTesting(TextureFormat depthBufferFormat, bool write = true, CompareOperation compareOp = CompareOperation.ReversedLess)
    {
        return EnableDepthTesting(depthBufferFormat, write, compareOp);
    }
    
    public GraphicsPipelineBuilder EnableDepthTesting(TextureFormat depthBufferFormat, bool write = true, CompareOperation compareOp = CompareOperation.Less)
    {
        if (depthBufferFormat == TextureFormat.None)
        {
            throw new ArgumentException($"{nameof(depthBufferFormat)} should be something else than {nameof(TextureFormat.None)} to be enabled");
        }
        
        _info.DepthBufferFormat = (DepthBufferFormat)depthBufferFormat;
        _info.SdlGpuDepthStencilState = _info.SdlGpuDepthStencilState with
        {
            enable_depth_test = true,
            enable_depth_write = write,
            compare_op = (SDL_GPUCompareOp)compareOp,
        };
        
        return this;
    }
    
    public GraphicsPipelineBuilder Custom(TextureFormat depthBufferFormat)
    {
        if (depthBufferFormat == TextureFormat.None)
        {
            throw new ArgumentException($"{nameof(depthBufferFormat)} should be something else than {nameof(TextureFormat.None)} to be enabled");
        }
        
        _info.DepthBufferFormat = (DepthBufferFormat)depthBufferFormat;
        _info.SdlGpuDepthStencilState = _info.SdlGpuDepthStencilState with
        {
            enable_depth_test = false,
            enable_depth_write = true,
            compare_op = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_NEVER,
        };
        
        return this;
    }

    public GraphicsPipelineBuilder EnableStencilTesting(in StencilOperationState frontFacing, CompareOperation compareOperation, byte compareMask=0xFF, byte writeMask=0xFF)
    {
        _info.SdlGpuDepthStencilState = _info.SdlGpuDepthStencilState with
        {
            enable_stencil_test = true,
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
            enable_stencil_test = true,
            compare_op = (SDL_GPUCompareOp)compareOperation,
            front_stencil_state = frontFacing,
            back_stencil_state = backFacing,
            compare_mask = compareMask,
            write_mask = writeMask
        };
        return this;
    }

    public GraphicsPipelineBuilder SetRasterizerState(RasterizerState rasterizerState)
    {
        _info.RasterizerState = rasterizerState;
        return this;
    }

    public GraphicsPipelineBuilder SetCullMode(CullMode cullMode)
    {
        _info.RasterizerState.CullMode = cullMode;
        return this;
    }

    public GraphicsPipelineBuilder SetFrontFace(FrontFace frontFace)
    {
        _info.RasterizerState.FrontFace = frontFace;
        return this;
    }
    
    public GraphicsPipeline Build()
    {
        Span<SDL_GPUColorTargetDescription> sdlGpuColorTargetDescriptions =
            CollectionsMarshal.AsSpan(_info.SdlGpuColorTargetDescriptions);
        Span<SDL_GPUVertexBufferDescription> sdlGpuVertexBufferDescription =
            CollectionsMarshal.AsSpan(_info.SdlGpuVertexBufferDescriptions);
        Span<SDL_GPUVertexAttribute> sdlGpuVertexAttributes = CollectionsMarshal.AsSpan(_info.SdlGpuVertexAttributes);

        if (_info.VertexBufferType == null)
        {
            // TODO: change
            throw new NotImplementedException();
        }

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
                    target_info = new SDL_GPUGraphicsPipelineTargetInfo
                    {
                        num_color_targets = (uint)sdlGpuColorTargetDescriptions.Length,
                        color_target_descriptions = sdlGpuColorTargetDescriptionsPointer,
                        has_depth_stencil_target = _info.DepthBufferFormat.HasValue,
                        depth_stencil_format = _info.DepthBufferFormat.HasValue ? (SDL_GPUTextureFormat)_info.DepthBufferFormat : default,
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
                    depth_stencil_state = _info.SdlGpuDepthStencilState,
                    rasterizer_state = new SDL_GPURasterizerState
                    {
                        fill_mode = SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL,
                        cull_mode = (SDL_GPUCullMode)_info.RasterizerState.CullMode,
                        front_face = (SDL_GPUFrontFace)_info.RasterizerState.FrontFace
                    }
                };

                var pipeline = SDL3.SDL_CreateGPUGraphicsPipeline(_gpuDevice.SdlGpuDevice, &sdlGpuGraphicsPipelineCreateInfo);
                if (pipeline == null)
                {
                    throw new GameKitInitializationException(
                        $"SDL_CreateGPUGraphicsPipeline failed: {SDL3.SDL_GetError()}");
                }

                GraphicsPipeline graphicsPipeline = new GraphicsPipeline(_gpuDevice, pipeline, _info.VertexBufferType);
                _info.Reset();
                
                _gpuDevice.RegisterGraphicsPipeline(graphicsPipeline);
                return graphicsPipeline;
            }
        }
    }
}
