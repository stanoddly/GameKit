using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

internal sealed class PencuilState<TRenderContext>
    where TRenderContext : IRenderContext
{
    internal Pencil Pencil { get; }
    internal ViewRegistry ViewRegistry { get; }
    internal PencuilOptions Options { get; }

    internal PencuilState(
        Pencil pencil,
        ViewRegistry viewRegistry,
        PencuilOptions options)
    {
        Pencil = pencil;
        ViewRegistry = viewRegistry;
        Options = options;
    }
}
