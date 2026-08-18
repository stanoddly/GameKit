using Pixely.Componentize;
using Pixely.DependencyInjection;

namespace Pixely.Pencuil;

public abstract class PencuilViewComponent<TViewModel> : GameComponent, IPencuilView
    where TViewModel : ComponentBase, IPencuilViewModel
{
    private readonly ViewScope _viewScope;
    private PencuilViewRegistry? _viewRegistry;

    protected TViewModel ViewModel { get; private set; } = default!;

    ViewScope IPencuilView.ViewScope => _viewScope;

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
        _ = Pencuil.GetRequired(pencuils, _viewScope);
        _viewRegistry = GetRequiredService<PencuilViewRegistry>();
        _viewRegistry.Add(this);
    }

    protected override void OnDetach()
    {
        _viewRegistry!.Remove(this);
        _viewRegistry = null;
    }
}
