namespace GameKit.Pencuil;

internal sealed class PencuilOptions : IViewScoped
{
    public ViewScope ViewScope { get; }
    public int Order { get; init; } = 10_000;
    public int InputOrder { get; init; } = -10_000;
    public bool ClearTarget { get; init; }

    internal PencuilOptions(ViewScope viewScope)
    {
        ViewScope = viewScope;
    }
}
