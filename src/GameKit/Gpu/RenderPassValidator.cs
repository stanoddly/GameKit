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
    void OnBindVertexBuffer<TVertexType>(RenderPass<TSelfValidator> renderPass, GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType;

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
}

/// <summary>
/// Validates render pass state with full validation checks.
/// </summary>
public struct RenderPassValidator : IRenderPassValidator<RenderPassValidator>
{
    private uint _verticesCount;
    private VertexTypeId _vertexBufferVertexType;
    private GraphicsPipeline? _graphicsPipeline;
    private readonly CommandBuffer _commandBuffer;

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
    }

    public void OnBindVertexBuffer<TVertexType>(RenderPass<RenderPassValidator> renderPass, GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType
    {
        _verticesCount = (uint)buffer.Size;
        _vertexBufferVertexType = VertexTypeId<TVertexType>.Value;
    }

    public void OnBindFragmentSamplers(RenderPass<RenderPassValidator> renderPass, uint slot, int samplerCount)
    {
    }

    public void OnDrawPrimitive(RenderPass<RenderPassValidator> renderPass)
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

    public void OnBindVertexBuffer<TVertexType>(RenderPass<NullRenderPassValidator> renderPass, GpuVertexBuffer<TVertexType> buffer)
        where TVertexType : unmanaged, IVertexType
    {
    }

    public void OnBindFragmentSamplers(RenderPass<NullRenderPassValidator> renderPass, uint slot, int samplerCount)
    {
    }

    public void OnDrawPrimitive(RenderPass<NullRenderPassValidator> renderPass)
    {
    }
}