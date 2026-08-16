using GameKit.Componentize;
using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public abstract class ViewComponent<TRenderContext, TViewModel> : GameComponent, IView<TRenderContext>
    where TRenderContext : IRenderContext
    where TViewModel : ComponentBase, IViewModel
{
    protected TViewModel ViewModel { get; private set; } = default!;

    public bool ConsumeDirty()
    {
        if (!ViewModel.IsDirty)
        {
            return false;
        }

        ViewModel.IsDirty = false;
        return true;
    }

    public abstract void Build(Pencil pencil);

    protected override void OnAttach()
    {
        ViewModel = GetSibling<TViewModel>();
        GetRequiredService<PencuilState<TRenderContext>>().ViewRegistry.Add(this);
    }

    protected override void OnDetach()
    {
        GetRequiredService<PencuilState<TRenderContext>>().ViewRegistry.Remove(this);
    }
}

public abstract class ViewComponent<TViewModel> : ViewComponent<DefaultRenderContext, TViewModel>
    where TViewModel : ComponentBase, IViewModel
{
}
