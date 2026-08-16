using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public interface IView
{
    bool ConsumeDirty();
    void Build(Pencil pencil);
}

public interface IView<TRenderContext> : IView
    where TRenderContext : IRenderContext
{
}
