using Pixely.DependencyInjection;

namespace Pixely.Pencuil;

internal sealed class Pencuil
{
    internal Pencil Pencil { get; }

    public ViewScope ViewScope { get; }

    internal Pencuil(ViewScope viewScope, Pencil pencil)
    {
        ViewScope = viewScope;
        Pencil = pencil;
    }

    internal static Pencuil GetRequired(ServiceProvider provider, ViewScope viewScope)
    {
        ServiceRegistry<Pencuil> registry = provider.GetRequiredService<ServiceRegistry<Pencuil>>();
        return GetRequired(registry, viewScope);
    }

    internal static Pencuil GetRequired(
        ServiceRegistry<Pencuil> registry,
        ViewScope viewScope)
    {
        Pencuil? result = null;

        foreach (Pencuil pencuil in registry)
        {
            if (pencuil.ViewScope != viewScope)
            {
                continue;
            }

            if (result != null)
            {
                throw new InvalidOperationException(
                    $"Pencuil is configured more than once for ViewScope {viewScope.Value}.");
            }

            result = pencuil;
        }

        return result ?? throw new InvalidOperationException(
            $"Pencuil is not configured for ViewScope {viewScope.Value}.");
    }
}
