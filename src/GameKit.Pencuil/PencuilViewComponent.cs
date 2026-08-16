using GameKit.Componentize;
using GameKit.DependencyInjection;

namespace GameKit.Pencuil;

public abstract class PencuilViewComponent<TViewModel> : GameComponent, IPencuilView
    where TViewModel : ComponentBase, IPencuilViewModel
{
    protected TViewModel ViewModel { get; private set; } = default!;

    public ViewScope ViewScope { get; }

    protected PencuilViewComponent(ViewScope viewScope)
    {
        ViewScope = viewScope;
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
        GetViewRegistry().Add(this);
    }

    protected override void OnDetach()
    {
        GetViewRegistry().Remove(this);
    }

    private PencuilViewRegistry GetViewRegistry()
    {
        ServiceRegistry<PencuilState> states =
            GetRequiredService<ServiceRegistry<PencuilState>>();
        foreach (PencuilState state in states)
        {
            if (state.ViewScope == ViewScope)
            {
                return state.ViewRegistry;
            }
        }

        throw new InvalidOperationException(
            $"Pencuil is not configured for ViewScope {ViewScope.Value}.");
    }
}
