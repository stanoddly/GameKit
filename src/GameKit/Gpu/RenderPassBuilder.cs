namespace GameKit.Gpu;

internal struct RenderPassBuilderState
{
    public RenderPassBuilderState()
    {
        ResetState();
    }

    public List<Texture> ColorTargets { get; } = new();
    public List<ColorTargetSettings> ColorTargetSettings { get; } = new();
    public Texture? DepthBuffer { get; set; }
    public DepthBufferSettings DepthBufferSettings { get; set; } = DepthBufferSettings.Default;
    public ColorTargetSettings? SharedColorTargetSettings { get; set; }

    public void ResetState()
    {
        ColorTargets.Clear();
        ColorTargetSettings.Clear();
        DepthBuffer = null;
        DepthBufferSettings = DepthBufferSettings.Default;
        SharedColorTargetSettings = null;
    }
}

public interface IRenderPassBuilder
{
    IRenderPassBuilder AddColorTarget(Texture texture);
    IRenderPassBuilder AddColorTarget(Texture texture, ColorTargetSettings settings);
    IRenderPassBuilder AddColorTargets(ReadOnlySpan<Texture> textures);
    IRenderPassBuilder SetSharedColorTargetSettings(ColorTargetSettings settings);
    IRenderPassBuilder SetDepthBuffer(Texture depthBuffer, DepthBufferSettings settings);

    IRenderPass Build();
}

public class RenderPassBuilder : IRenderPassBuilder
{
    private RenderPassBuilderState _state = new();
    private readonly ICommandBuffer _commandBuffer;

    public RenderPassBuilder(ICommandBuffer commandBuffer)
    {
        _commandBuffer = commandBuffer;
    }
    
    public IRenderPassBuilder AddColorTarget(Texture texture)
    {
        _state.ColorTargets.Add(texture);
        return this;
    }
    
    public IRenderPassBuilder AddColorTargets(ReadOnlySpan<Texture> textures)
    {
        foreach (var texture in textures)
        {
            AddColorTarget(texture);
        }
        return this;
    }

    public IRenderPassBuilder AddColorTarget(Texture texture, ColorTargetSettings settings)
    {
        _state.ColorTargets.Add(texture);
        _state.ColorTargetSettings.Add(settings);
        return this;
    }

    public IRenderPassBuilder SetSharedColorTargetSettings(ColorTargetSettings settings)
    {
        _state.SharedColorTargetSettings = settings;
        return this;
    }

    public IRenderPassBuilder SetDepthBuffer(Texture depthBuffer, DepthBufferSettings settings)
    {
        _state.DepthBuffer = depthBuffer;
        _state.DepthBufferSettings = settings;
        return this;
    }

    public IRenderPass Build()
    {
        bool hasShared = _state.SharedColorTargetSettings != null;
        bool hasPerTarget = _state.ColorTargetSettings.Count > 0;

        if (hasShared && hasPerTarget)
        {
            throw new InvalidOperationException("Cannot have both shared and per-target settings set at once.");
        }

        if (!hasShared && !hasPerTarget)
        {
            throw new InvalidOperationException("Must have either shared or per-target settings set.");
        }

        if (_state.ColorTargets.Count == 0)
        {
            throw new InvalidOperationException("At least one color target is required.");
        }

        if (hasShared)
        {
            for (int i = 0; i < _state.ColorTargets.Count; i++)
            {
                _state.ColorTargetSettings.Add(_state.SharedColorTargetSettings!);
            }
        }

        IRenderPass renderPass = _commandBuffer.CreateRenderPass(_state.ColorTargets, _state.ColorTargetSettings, _state.DepthBuffer,
            _state.DepthBufferSettings);

        _state.ResetState();

        return renderPass;
    }
}

