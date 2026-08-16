namespace GameKit.Pencuil;

public abstract class PencuilView<TViewModel> : IPencuilView
    where TViewModel : IPencuilViewModel
{
    protected TViewModel ViewModel { get; }

    public ViewScope ViewScope { get; }

    protected PencuilView(ViewScope viewScope, TViewModel viewModel)
    {
        ViewScope = viewScope;
        ViewModel = viewModel;
    }

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
}
