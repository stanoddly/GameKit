using GameKit.RenderOrchestration;
using Yak;

namespace GameKit.Pencuil;

[Module]
public interface IPencuil<TRenderContext>
    where TRenderContext : IRenderContext
{
    // Consumer-provided
    PencuilOptions PencuilOptions { get; }
    GuiStyle GuiStyle { get; }
    List<IView> Views { get; }

    [Singleton]
    Pencil Pencil { get; }

    [Singleton]
    ViewRegistry ViewRegistry { get; }

    [Singleton]
    PencuilRenderer PencuilRenderer { get; }

    [Singleton]
    PencuilRenderPhase<TRenderContext> PencuilRenderPhase { get; }
}
