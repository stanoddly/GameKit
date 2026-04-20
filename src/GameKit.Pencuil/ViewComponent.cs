using GameKit.Componentize;

namespace GameKit.Pencuil;

public abstract class ViewComponent<TViewModel> : GameComponent, IView where TViewModel : ComponentBase, IViewModel
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
        GetRequiredService<ViewRegistry>().Add(this);
    }

    protected override void OnDetach()
    {
        GetRequiredService<ViewRegistry>().Remove(this);
    }
}
