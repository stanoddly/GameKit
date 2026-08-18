namespace Pixely.Gpu;

public class DepthBufferSettings
{
    // 1.0f means farthest depth value
    public float ClearDepthValue { get; init; } = 1.0f;
    public LoadOperation DepthBufferLoadOperation { get; init; } = LoadOperation.Clear;
    public StoreOperation DepthBufferStoreOperation { get; init; } = StoreOperation.Store;
    public LoadOperation StencilLoadOperation { get; init; } = LoadOperation.Load;
    public StoreOperation StencilStoreOperation { get; init; } = StoreOperation.Store;
    public byte ClearStencilValue { get; init; } = 0;

    public static readonly DepthBufferSettings Default = new();

    public static readonly DepthBufferSettings ReversedDefault = new()
    {
        ClearDepthValue = 0.0f
    };
}