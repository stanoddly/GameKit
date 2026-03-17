using GameKit.Componentize;

namespace GameKit.Pencuil;

public abstract class ViewComponent<TViewModel> : GameComponent, IView where TViewModel : GameComponent, IViewModel
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
        Services<ViewRegistry>.Instance.Add(this);
    }

    protected override void OnDetach()
    {
        Services<ViewRegistry>.Instance.Remove(this);
    }
}
