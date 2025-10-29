namespace GameKit.Gpu;

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
}
