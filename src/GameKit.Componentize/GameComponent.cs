using GameKit.DependencyInjection;

namespace GameKit.Componentize;

public abstract class GameComponent
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
