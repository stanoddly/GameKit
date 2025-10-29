namespace GameKit.Gpu;

public class ColorTargetSettings
{
    public FColor ClearColorValue { get; init; } = FColors.Grey;
    public LoadOperation LoadOperation { get; init; } = LoadOperation.Clear;
    public StoreOperation StoreOperation { get; init; } = StoreOperation.Store;
    public static readonly ColorTargetSettings Clear = new();
}