namespace Pixely.Pencuil;

public abstract class PencuilView<TViewModel> : IPencuilView
    where TViewModel : IPencuilViewModel
{
    private readonly ViewScope _viewScope;

    protected TViewModel ViewModel { get; }

    ViewScope IPencuilView.ViewScope => _viewScope;

    protected PencuilView(TViewModel viewModel)
        : this(default, viewModel)
    {
    }

    protected PencuilView(ViewScope viewScope, TViewModel viewModel)
    {
        _viewScope = viewScope;
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
