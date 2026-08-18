namespace Pixely.Gpu;

public readonly record struct BlendingState(
    BlendFactor SourceColorBlendFactor,
    BlendFactor DestinationColorBlendFactor,
    BlendOp ColorBlendOp,
    BlendFactor SourceAlphaBlendFactor,
    BlendFactor DestinationAlphaBlendFactor,
    BlendOp AlphaBlendOp)
{
    public static readonly BlendingState Standard = new(
        SourceColorBlendFactor: BlendFactor.SrcAlpha,
        DestinationColorBlendFactor: BlendFactor.OneMinusSrcAlpha,
        ColorBlendOp: BlendOp.Add,
        SourceAlphaBlendFactor: BlendFactor.One,
        DestinationAlphaBlendFactor: BlendFactor.OneMinusSrcAlpha,
        AlphaBlendOp: BlendOp.Add
    );
    
    public static readonly BlendingState Additive = new(
        SourceColorBlendFactor: BlendFactor.One,
        DestinationColorBlendFactor: BlendFactor.One,
        ColorBlendOp: BlendOp.Add,
        SourceAlphaBlendFactor: BlendFactor.One,
        DestinationAlphaBlendFactor: BlendFactor.Zero,
        AlphaBlendOp: BlendOp.Add
    );
    
    public static readonly BlendingState Multiplicative = new(
        SourceColorBlendFactor: BlendFactor.Zero,
        DestinationColorBlendFactor: BlendFactor.SrcColor,
        ColorBlendOp: BlendOp.Add,
        SourceAlphaBlendFactor: BlendFactor.Zero,
        DestinationAlphaBlendFactor: BlendFactor.One,
        AlphaBlendOp: BlendOp.Add
    );

    /// <summary>
    /// Premultiplied alpha blending. Use when source RGB is already multiplied by alpha,
    /// or when the shader outputs premultiplied alpha. This handles "dirty" transparent
    /// pixels correctly by not multiplying source color by alpha again.
    /// </summary>
    public static readonly BlendingState PremultipliedAlpha = new(
        SourceColorBlendFactor: BlendFactor.One,
        DestinationColorBlendFactor: BlendFactor.OneMinusSrcAlpha,
        ColorBlendOp: BlendOp.Add,
        SourceAlphaBlendFactor: BlendFactor.One,
        DestinationAlphaBlendFactor: BlendFactor.OneMinusSrcAlpha,
        AlphaBlendOp: BlendOp.Add
    );
}
