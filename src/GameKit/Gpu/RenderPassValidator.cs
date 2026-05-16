using GameKit.ShaderCommon;

namespace GameKit.Gpu;

public interface IRenderPassValidator<TSelfValidator> where TSelfValidator: IRenderPassValidator<TSelfValidator>
{
    static abstract TSelfValidator Create(CommandBuffer commandBuffer);

    /// <summary>
    /// Called when a graphics pipeline is bound to the render pass.
    /// </summary>
    void OnBindGraphicsPipeline(RenderPass<TSelfValidator> renderPass, GraphicsPipeline graphicsPipeline);

    /// <summary>
    /// Called when a vertex buffer is bound to the render pass.
    /// </summary>
    void OnBindVertexBuffer<TVertexType>(RenderPass<TSelfValidator> renderPass, uint slot, GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType;

    /// <summary>
    /// Called when an index buffer is bound to the render pass.
    /// </summary>
    void OnBindIndexBuffer(RenderPass<TSelfValidator> renderPass, GpuIndexBuffer buffer);

    /// <summary>
    /// Called when vertex samplers are bound to the render pass.
    /// </summary>
    void OnBindVertexSamplers(RenderPass<TSelfValidator> renderPass, uint slot, int samplerCount);

    /// <summary>
    /// Called when fragment samplers are bound to the render pass.
    /// </summary>
    void OnBindFragmentSamplers(RenderPass<TSelfValidator> renderPass, uint slot, int samplerCount);

    /// <summary>
    /// Called when a primitive draw is requested.
    /// Validates that the current render pass state is valid for drawing.
    /// Throws an exception if validation fails.
    /// </summary>
    void OnDrawPrimitive(RenderPass<TSelfValidator> renderPass);

    /// <summary>
    /// Called when an indexed primitive draw is requested.
    /// Validates that the current render pass state is valid for drawing.
    /// Throws an exception if validation fails.
    /// </summary>
    void OnDrawIndexedPrimitive(RenderPass<TSelfValidator> renderPass);
}

/// <summary>
/// Validates render pass state with full validation checks.
/// </summary>
public struct RenderPassValidator : IRenderPassValidator<RenderPassValidator>
{
    private const int MaxVertexBufferSlots = 8;

    private uint _verticesCount;
    private GpuIndexBuffer? _indexBuffer;
    private GraphicsPipeline? _graphicsPipeline;
    private readonly CommandBuffer _commandBuffer;

    // Track bound vertex types per slot (up to 8 slots should be plenty)
    private VertexTypeId _slot0Type;
    private VertexTypeId _slot1Type;
    private VertexTypeId _slot2Type;
    private VertexTypeId _slot3Type;
    private VertexTypeId _slot4Type;
    private VertexTypeId _slot5Type;
    private VertexTypeId _slot6Type;
    private VertexTypeId _slot7Type;

    private RenderPassValidator(CommandBuffer commandBuffer)
    {
        _commandBuffer = commandBuffer;
    }

    public static RenderPassValidator Create(CommandBuffer commandBuffer)
    {
        return new RenderPassValidator(commandBuffer);
    }

    public void OnBindGraphicsPipeline(RenderPass<RenderPassValidator> renderPass, GraphicsPipeline graphicsPipeline)
    {
        _graphicsPipeline = graphicsPipeline;

        DepthBufferFormat renderPassFormat = renderPass.DepthBufferFormat;
        DepthBufferFormat pipelineFormat = graphicsPipeline.DepthBufferFormat;

        if (renderPassFormat != pipelineFormat)
        {
            throw new InvalidOperationException(
                $"Depth/stencil format mismatch: the render pass uses {renderPassFormat} but the pipeline was created with {pipelineFormat}. " +
                $"Ensure the depth buffer format passed to EnableDepthTesting matches the format of the depth buffer texture used in the render pass.");
        }

        // Reset slot bindings when pipeline changes
        _slot0Type = VertexTypeId.Null;
        _slot1Type = VertexTypeId.Null;
        _slot2Type = VertexTypeId.Null;
        _slot3Type = VertexTypeId.Null;
        _slot4Type = VertexTypeId.Null;
        _slot5Type = VertexTypeId.Null;
        _slot6Type = VertexTypeId.Null;
        _slot7Type = VertexTypeId.Null;
    }

    public void OnBindVertexBuffer<TVertexType>(RenderPass<RenderPassValidator> renderPass, uint slot, GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType
    {
        if (slot >= MaxVertexBufferSlots)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), $"Slot must be less than {MaxVertexBufferSlots}.");
        }

        if (slot == 0)
        {
            _verticesCount = (uint)buffer.Size;
        }

        VertexTypeId typeId = VertexTypeId<TVertexType>.Value;
        SetSlotType(slot, typeId);
    }

    public void OnBindIndexBuffer(RenderPass<RenderPassValidator> renderPass, GpuIndexBuffer buffer)
    {
        _indexBuffer = buffer;
    }

    private void SetSlotType(uint slot, VertexTypeId typeId)
    {
        switch (slot)
        {
            case 0: _slot0Type = typeId; break;
            case 1: _slot1Type = typeId; break;
            case 2: _slot2Type = typeId; break;
            case 3: _slot3Type = typeId; break;
            case 4: _slot4Type = typeId; break;
            case 5: _slot5Type = typeId; break;
            case 6: _slot6Type = typeId; break;
            case 7: _slot7Type = typeId; break;
        }
    }

    private readonly VertexTypeId GetSlotType(uint slot)
    {
        return slot switch
        {
            0 => _slot0Type,
            1 => _slot1Type,
            2 => _slot2Type,
            3 => _slot3Type,
            4 => _slot4Type,
            5 => _slot5Type,
            6 => _slot6Type,
            7 => _slot7Type,
            _ => VertexTypeId.Null
        };
    }

    public void OnBindVertexSamplers(RenderPass<RenderPassValidator> renderPass, uint slot, int samplerCount)
    {
    }

    public void OnBindFragmentSamplers(RenderPass<RenderPassValidator> renderPass, uint slot, int samplerCount)
    {
    }

    public void OnDrawPrimitive(RenderPass<RenderPassValidator> renderPass)
    {
        ValidateDrawState(renderPass);
    }

    public void OnDrawIndexedPrimitive(RenderPass<RenderPassValidator> renderPass)
    {
        ValidateDrawState(renderPass);

        if (_indexBuffer == null)
        {
            throw new InvalidOperationException("IndexBuffer must be bound before indexed drawing.");
        }

        if (_indexBuffer.Size == 0)
        {
            throw new InvalidOperationException("Bound IndexBuffer is empty.");
        }
    }

    private void ValidateDrawState(RenderPass<RenderPassValidator> renderPass)
    {
        if (_graphicsPipeline == null)
        {
            throw new InvalidOperationException(
                $"{nameof(GraphicsPipeline)} must be bound.");
        }

        // Validate all configured slots have matching buffer types
        for (int i = 0; i < _graphicsPipeline.VertexBufferSlotCount; i++)
        {
            VertexTypeId expectedType = _graphicsPipeline.VertexBufferTypeIds[i];
            VertexTypeId boundType = GetSlotType((uint)i);

            if (boundType == VertexTypeId.Null)
            {
                throw new InvalidOperationException(
                    $"Vertex buffer slot {i} is not bound. Pipeline expects {_graphicsPipeline.VertexBufferSlotCount} buffer(s).");
            }

            if (expectedType != boundType)
            {
                throw new InvalidOperationException(
                    $"Vertex buffer type mismatch at slot {i}. Pipeline expects a different vertex type.");
            }
        }

        if (_verticesCount == 0)
        {
            throw new InvalidOperationException("Bound VertexBuffer at slot 0 is empty.");
        }

        ShaderBindingLayoutValidator.ValidateBindingCounts(_graphicsPipeline.FragmentShader.BindingLayout.BindingCounts,
            renderPass.FragmentShaderBindingCounts);

        ShaderBindingLayoutValidator.ValidateUniformSlotSizes(_graphicsPipeline.FragmentShader.BindingLayout.UniformSlotSizes,
            _commandBuffer.FragmentShaderUniformSlotSizes);

        ShaderBindingLayoutValidator.ValidateUniformSlotSizes(_graphicsPipeline.VertexShader.BindingLayout.UniformSlotSizes,
            _commandBuffer.VertexShaderUniformSlotSizes);
    }
}

/// <summary>
/// No-op validator that performs no validation. Useful for release builds or performance-critical code.
/// </summary>
public struct NullRenderPassValidator : IRenderPassValidator<NullRenderPassValidator>
{
    public static NullRenderPassValidator Create(CommandBuffer commandBuffer)
    {
        return new NullRenderPassValidator();
    }

    public void OnBindGraphicsPipeline(RenderPass<NullRenderPassValidator> renderPass, GraphicsPipeline graphicsPipeline)
    {
    }

    public void OnBindVertexBuffer<TVertexType>(RenderPass<NullRenderPassValidator> renderPass, uint slot, GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType
    {
    }

    public void OnBindIndexBuffer(RenderPass<NullRenderPassValidator> renderPass, GpuIndexBuffer buffer)
    {
    }

    public void OnBindVertexSamplers(RenderPass<NullRenderPassValidator> renderPass, uint slot, int samplerCount)
    {
    }

    public void OnBindFragmentSamplers(RenderPass<NullRenderPassValidator> renderPass, uint slot, int samplerCount)
    {
    }

    public void OnDrawPrimitive(RenderPass<NullRenderPassValidator> renderPass)
    {
    }

    public void OnDrawIndexedPrimitive(RenderPass<NullRenderPassValidator> renderPass)
    {
    }
}
