using GameKit.Componentize;
using GameKit.DependencyInjection;

namespace GameKit.Pencuil;

public abstract class PencuilViewComponent<TViewModel> : GameComponent, IPencuilView
    where TViewModel : ComponentBase, IPencuilViewModel
{
    private readonly ViewScope _viewScope;
    private Pencuil? _pencuil;

    protected TViewModel ViewModel { get; private set; } = default!;

    ViewScope IViewScoped.ViewScope => _viewScope;

    protected PencuilViewComponent()
    {
    }

    protected PencuilViewComponent(ViewScope viewScope)
    {
        _viewScope = viewScope;
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

    protected override void OnAttach()
    {
        ViewModel = GetSibling<TViewModel>();
        ServiceRegistry<Pencuil> pencuils = GetRequiredService<ServiceRegistry<Pencuil>>();
        _pencuil = Pencuil.GetRequired(pencuils, _viewScope);
        _pencuil.AddComponentView(this);
    }

    protected override void OnDetach()
    {
        _pencuil!.RemoveComponentView(this);
        _pencuil = null;
    }
}
