using GameKit.RenderOrchestration;

namespace GameKit.Pencuil;

public abstract class View<TRenderContext, TViewModel> : IView<TRenderContext>
    where TRenderContext : IRenderContext
    where TViewModel : IViewModel
{
    protected TViewModel ViewModel { get; }

    public bool ConsumeDirty()
    {
        if (!ViewModel.IsDirty)
        {
            return false;
        }

        ViewModel.IsDirty = false;
        return true;
    }

    protected View(TViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    public abstract void Build(Pencil pencil);
}

public abstract class View<TViewModel> : View<DefaultRenderContext, TViewModel>
    where TViewModel : IViewModel
{
    protected View(TViewModel viewModel)
        : base(viewModel)
    {
    }
}
