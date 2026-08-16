namespace GameKit.Pencuil;

internal sealed class PencuilState : IViewScoped
{
    internal Pencil Pencil { get; }
    internal PencuilViewRegistry ViewRegistry { get; }
    internal PencuilOptions Options { get; }

    public ViewScope ViewScope { get; }

    internal PencuilState(
        ViewScope viewScope,
        Pencil pencil,
        PencuilViewRegistry viewRegistry,
        PencuilOptions options)
    {
        ViewScope = viewScope;
        Pencil = pencil;
        ViewRegistry = viewRegistry;
        Options = options;
    }
}
