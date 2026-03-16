namespace GameKit.Pencuil;

public abstract class View<TViewModel> : IView where TViewModel : IViewModel
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
