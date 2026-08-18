using Pixely.DependencyInjection;

namespace Pixely.Componentize;

public abstract class ComponentBase
{
    protected internal virtual void OnAttach(GameObject owner, ServiceProvider services)
    {

    }

    protected internal virtual void OnReady(GameObject owner, ServiceProvider services)
    {

    }

    protected internal virtual void OnDetach(GameObject owner, ServiceProvider services)
    {

    }
}
